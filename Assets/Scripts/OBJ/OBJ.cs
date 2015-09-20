using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Net;

public class OBJ : MonoBehaviour {
	
    public Text text = null;
    public string obj_path;
    protected string mtl_path = "model.mtl";
    protected string img_path = "model.png";
#if STANDALONE_DEBUG
    public int myPort = 3850;
    protected Socket serverSocket;
    protected List<Socket> clientSocket;
    protected byte[] request = new byte[2048];
    /*socket command*/
    protected const string LOAD = "Load";
    protected const string LOADLOCAL = "LoadLocal";
#endif
    protected bool show_envelop, show_joint, show_body;

	/* OBJ file tags */
	private const string O 	= "o";
	private const string G 	= "g";
	private const string V 	= "v";
	private const string VT = "vt";
	private const string VN = "vn";
	private const string F 	= "f";
	private const string MTL = "mtllib";
	private const string UML = "usemtl";

	/* MTL file tags */
	private const string NML = "newmtl";
	private const string NS = "Ns"; // Shininess
	private const string KA = "Ka"; // Ambient component (not supported)
	private const string KD = "Kd"; // Diffuse component
	private const string KS = "Ks"; // Specular component
	private const string D = "d"; 	// Transparency (not supported)
	private const string TR = "Tr";	// Same as 'd'
	private const string ILLUM = "illum"; // Illumination model. 1 - diffuse, 2 - specular
	private const string MAP_KD = "map_Kd"; // Diffuse texture (other textures are not supported)

	private string mtllib;
    protected GeometryBuffer buffer;
    protected MovePara move_para;

    public enum LoadState {
        LOADOBJ,
        IDLE,
        RUNNING        
    };
    public LoadState load_state;

	void Awake ()
	{                
        /*
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        objPath = "file://" + Application.streamingAssetsPath + "/model2.obj";
#else
#if UNITY_ANDROID
        FileInfo fi = new FileInfo("/sdcard/model.obj");
        if (fi.Exists)
            objPath = "file://" +"/sdcard/model.obj";
        else
            objPath = Application.streamingAssetsPath + "/model2.obj";            
#endif
#endif
         */
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        obj_path = "f:/chenyu/Unity/paoku/model.obj";
#else
#if UNITY_ANDROID
        obj_path = Application.persistentDataPath +"/model.obj";
#endif
#endif
#if STANDALONE_DEBUG
        IPAddress ip = IPAddress.Parse("127.0.0.1");
        serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        serverSocket.Bind(new IPEndPoint(ip, myPort));
        serverSocket.Listen(5);
        clientSocket = new List<Socket>();
#endif
        load_state = LoadState.IDLE;
        move_para = new MovePara();
        show_envelop = false;
        show_joint = false;
        show_body = true;

        gameObject.AddComponent<Animation>();        
        animation.AddClip(move_para.create_move(Movement.RUN), "Idle_1");
        animation.AddClip(move_para.create_move(Movement.RUN), "Idle_2");
        animation.AddClip(move_para.create_move(Movement.RUN, 90), "run");
        animation.AddClip(move_para.create_move(Movement.JUMP), "jump");
        animation.AddClip(move_para.create_move(Movement.RUN), "right");
        animation.AddClip(move_para.create_move(Movement.RUN), "left");
        animation.AddClip(move_para.create_move(Movement.RUN), "death");
        animation.AddClip(move_para.create_move(Movement.SLIDE), "slide");
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        LoadObj("file://" + Application.streamingAssetsPath + "/model2.obj");
#else
#if UNITY_ANDROID
        LoadObj("file://" + "/sdcard/model.obj");
#endif
#endif
    }

    void Update()
    {        
#if STANDALONE_DEBUG
        int i;
        if (serverSocket.Poll(0, SelectMode.SelectRead))
        {
            Debug.Log("Accept new connection");
            clientSocket.Add(serverSocket.Accept());
        }
            

        for (i = 0; i < clientSocket.Count; i++)
            if (clientSocket[i].Poll(0, SelectMode.SelectRead))
                break;

        string req_str = null;

        if (i != clientSocket.Count)
        {
            try
            {
                int req_len = clientSocket[i].Receive(request);
                if (req_len == 0)
                {
                    clientSocket[i].Close();
                    clientSocket.RemoveAt(i);
                }
                else
                    req_str = System.Text.Encoding.ASCII.GetString(request, 0, req_len);                    
            }
            catch
            {
                Debug.Log("remote socket " + i + " closed");                
                clientSocket.RemoveAt(i);
            }
        }

        if (req_str != null)
        {
            string [] cmd_line = req_str.Split("\n".ToCharArray());
            foreach (string cmd in cmd_line) {
                if (cmd == "")
                    continue;
                Debug.Log("receive command: " + cmd); 
                string[] cmd_arg = cmd.Split(" ".ToCharArray());

                switch (cmd_arg[0])
                {
                    case LOAD:
                        LoadObj(cmd_arg[1]);
                        break;

                    case LOADLOCAL:
                        LoadObj(cmd_arg[1], true);
                        break;
                }
            }
        }
#endif
        if (Input.GetKeyDown(KeyCode.Alpha1))        
            show_hide_envelop();

        if (Input.GetKeyDown(KeyCode.Alpha2))
            show_hide_joint();

        if (Input.GetKeyDown(KeyCode.Alpha3))
            show_hide_body();
      
        SkinnedMeshRenderer renderer = GetComponent<SkinnedMeshRenderer>();
        if (load_state == LoadState.LOADOBJ || renderer.bones == null || renderer.bones.Length==0)
            return;
        
        if (load_state == LoadState.IDLE)
        {
            Transform[] bones = renderer.bones;
            Quaternion[] rot;
            rot = buffer.normal_rot;
            rot[GenerateBone.HIP] = Quaternion.identity;
            GenerateBone.apply_posture(rot, bones);
            renderer.bones = bones;
        }                
        
    }

    void OnDestroy()
    {
#if STANDALONE_DEBUG
        foreach (Socket s in clientSocket)
        {
            string str = "Goodbye";
            s.Send(System.Text.Encoding.ASCII.GetBytes(str));
            s.Close();
        }
#endif
    }

    public void LoadObj(string path, bool load_local=false)
    {
        if (load_state == LoadState.LOADOBJ)
        {
            Debug.LogWarning("drop command" + path);
            return;
        }

        if (load_state != LoadState.IDLE)
        {
            load_state = LoadState.IDLE;
            animation.Stop();            
        }            
        load_state = LoadState.LOADOBJ;
        StartCoroutine(Load(path, load_local));        
    }
	
    public void show_hide_envelop()
    {
        if (buffer != null && load_state != LoadState.LOADOBJ)
        {
            show_envelop = !show_envelop;            
            buffer.show_envelop(show_envelop);
        }
    }

    public void show_hide_joint()
    {
        if (buffer != null && load_state != LoadState.LOADOBJ)
        {            
            show_joint = !show_joint;
            buffer.show_joint(show_joint);
        }
    }

    public void show_hide_body()
    {
        if (buffer != null && load_state != LoadState.LOADOBJ)
        {
            show_body = !show_body;
            renderer.enabled = show_body;
        }
    }

	public IEnumerator Load(string path, bool load_local) {
        bool update_local = !load_local;
        if (load_local)
        {
            if (!obj_path.StartsWith("file:"))
                path = "file://" + obj_path;
        }     
            
        
        mtllib = null;
            
        renderer.enabled = false;
        if (buffer != null)
            buffer.Release();
        buffer = new GeometryBuffer();
        
		string basepath = (path.IndexOf("/") == -1) ? "" : path.Substring(0, path.LastIndexOf("/") + 1);
        if (text != null)
        {
            text.enabled = true;
            text.text = "loading " + path;
        }        
        Debug.Log(DateTime.Now.Second + "." + DateTime.Now.Millisecond + "paoku load obj:" + path);
        WWW loader = new WWW(path);        
        yield return loader;

        if (loader.error != null)
        {
            Debug.Log(DateTime.Now.Second + "paoku load model failed" + loader.error);
            if (text!=null)
                text.text = loader.error;            
            yield break;
        }
            
        Debug.Log(DateTime.Now.Second + "." + DateTime.Now.Millisecond + "paoku is decoding obj file");
        SetGeometryData(loader.text);                 
        if (update_local)
        {
            Debug.Log("update local obj:" + obj_path);
            FileStream fileStream = new FileStream(obj_path, FileMode.Create);
            fileStream.Write(loader.bytes, 0, loader.bytes.Length);
            fileStream.Close();
        } 

		if(hasMaterials) {
            if (load_local)
                mtllib = mtl_path;
            if (mtllib.StartsWith("./"))
                mtllib = mtllib.Substring(2);
            if (text != null)
                text.text = "loading " + basepath + mtllib;
            Debug.Log("paoku load material:" + basepath + mtllib);
            loader = new WWW(basepath + mtllib);
            yield return loader;
            if (loader.text == null)
            {
                Debug.Log("paoku load material fail");
                yield break;
            }

            SetMaterialData(loader.text);
            if (update_local)
            {
                string bpath = (obj_path.IndexOf("/") == -1) ? "" : obj_path.Substring(0, obj_path.LastIndexOf("/") + 1);
                Debug.Log("update local material:" + bpath + mtl_path);
                FileStream fileStream = new FileStream(bpath+mtl_path, FileMode.Create);
                fileStream.Write(loader.bytes, 0, loader.bytes.Length);
                fileStream.Close();
            } 
			
			foreach(MaterialData m in materialData) {
                if (load_local)
                    m.diffuseTexPath = img_path;
				if(m.diffuseTexPath != null) {
                    if (text!=null)
                        text.text = "loading " + basepath + m.diffuseTexPath;
                    Debug.Log("paoku load texture:" + basepath + m.diffuseTexPath);
					WWW texloader = new WWW(basepath + m.diffuseTexPath);
					yield return texloader;
                    if (texloader.texture == null)
                    {
                        Debug.Log("paoku load texture fail");
                        yield break;
                    }                        
					m.diffuseTex = texloader.texture;
                    if (update_local)
                    {
                        string bpath = (obj_path.IndexOf("/") == -1) ? "" : obj_path.Substring(0, obj_path.LastIndexOf("/") + 1);
                        Debug.Log("update local texture:" + bpath + img_path);
                        FileStream fileStream = new FileStream(bpath+img_path, FileMode.Create);
                        fileStream.Write(texloader.bytes, 0, texloader.bytes.Length);
                        fileStream.Close();
                    }                    
				}
			}
		}
        if (text!=null)
            text.enabled = false;		
        
		Build();
        renderer.enabled = true;
        
#if STANDALONE_DEBUG
        CameraMove3D c3d = GameObject.Find("Main Camera").GetComponent("CameraMove3D") as CameraMove3D;
        c3d.StartObserve();
#endif
        
        load_state = LoadState.IDLE;
	}

	private void SetGeometryData(string data) {
		string[] lines = data.Split("\n".ToCharArray());
        Debug.Log(DateTime.Now.Second + "." + DateTime.Now.Millisecond + " split complete");

		for(int i = 0; i < lines.Length; i++) {            
			string l = lines[i];
			
			if(l.IndexOf("#") != -1) l = l.Substring(0, l.IndexOf("#"));
			string[] p = l.Split(" ".ToCharArray());
			
			switch(p[0]) {
				case O:
					buffer.PushObject(p[1].Trim());
					break;
				case G:
					buffer.PushGroup(p[1].Trim());
					break;
				case V:
					buffer.PushVertex( new Vector3( cf(p[1]), cf(p[2]), cf(p[3]) ) );
					break;
				case VT:
					buffer.PushUV(new Vector2( cf(p[1]), cf(p[2]) ));
					break;
				case VN:
					buffer.PushNormal(new Vector3( cf(p[1]), cf(p[2]), cf(p[3]) ));
					break;
				case F:
					for(int j = 1; j < p.Length; j++) {
						string[] c = p[j].Trim().Split("/".ToCharArray());
						FaceIndices fi = new FaceIndices();
						fi.vi = (ushort) (ci(c[0])-1);
                        if (c.Length > 1 && c[1] != "") fi.vu = (ushort) (ci(c[1]) - 1);
                        if (c.Length > 2 && c[2] != "") fi.vn = (ushort) (ci(c[2]) - 1);
						buffer.PushFace(ref fi);
					}
					break;
				case MTL:
					mtllib = p[1].Trim();
					break;
				case UML:
					buffer.PushMaterialName(p[1].Trim());
					break;
			}
		}
		
		// buffer.Trace();
	}
	
	private float cf(string v) {
        return float.Parse(v);    
	}
	
	private int ci(string v) {
        return int.Parse(v);
	}
	
	private bool hasMaterials {
		get {
			return mtllib != null;
		}
	}
	
	/* ############## MATERIALS */
	private List<MaterialData> materialData;
	private class MaterialData {
		public string name;
		public Color ambient;
   		public Color diffuse;
   		public Color specular;
   		public float shininess;
   		public float alpha;
   		public int illumType;
   		public string diffuseTexPath;
   		public Texture2D diffuseTex;
	}
	
	private void SetMaterialData(string data) {
		string[] lines = data.Split("\n".ToCharArray());
		
		materialData = new List<MaterialData>();
		MaterialData current = new MaterialData();
		
		for(int i = 0; i < lines.Length; i++) {
			string l = lines[i];
			
			if(l.IndexOf("#") != -1) l = l.Substring(0, l.IndexOf("#"));
			string[] p = l.Split(" ".ToCharArray());
			
			switch(p[0]) {
				case NML:
					current = new MaterialData();
					current.name = p[1].Trim();
					materialData.Add(current);
					break;
				case KA:
					current.ambient = gc(p);
					break;
				case KD:
					current.diffuse = gc(p);
					break;
				case KS:
					current.specular = gc(p);
					break;
				case NS:
					current.shininess = cf(p[1]) / 1000;
					break;
				case D:
				case TR:
					current.alpha = cf(p[1]);
					break;
				case MAP_KD:
					current.diffuseTexPath = p[1].Trim();
					break;
				case ILLUM:
					current.illumType = ci(p[1]);
					break;
					
			}
		}	
	}
	
	private Material GetMaterial(MaterialData md) {
		Material m;
		
		m =  new Material(Shader.Find("Diffuse"));
		

		m.SetColor("_Color", md.diffuse);

        md.diffuseTex.Apply();
		if(md.diffuseTex != null) m.SetTexture("_MainTex", md.diffuseTex);
        Debug.Log("paoku load texture wide=" + md.diffuseTex.width + ",height=" + md.diffuseTex.height);
        
		return m;
	}
	
	private Color gc(string[] p) {
		return new Color( cf(p[1]), cf(p[2]), cf(p[3]) );
	}

	private void Build() {
		Dictionary<string, Material> materials = new Dictionary<string, Material>();
		
		if(hasMaterials) {
			foreach(MaterialData md in materialData) {
				materials.Add(md.name, GetMaterial(md));
			}
		} else {
			materials.Add("default", new Material(Shader.Find("VertexLit")));
		}
		
		GameObject[] ms = new GameObject[buffer.numObjects];
		
		if(buffer.numObjects == 1) {            			
			ms[0] = gameObject;               
		} else if(buffer.numObjects > 1) {
			for(int i = 0; i < buffer.numObjects; i++) {
				GameObject go = new GameObject();
				go.transform.parent = gameObject.transform;                
				ms[i] = go;
			}
		}

        Debug.Log(DateTime.Now.Second + "." + DateTime.Now.Millisecond + "populate mesh start");
        buffer.PopulateMeshes(ms, materials, MovePara.normalHeight);
        Debug.Log(DateTime.Now.Second + "." + DateTime.Now.Millisecond + "populate mesh end");
	}
    
    public void start_stop_run()
    {
        if (load_state == LoadState.IDLE) 
        {
            load_state = LoadState.RUNNING;
            animation.Play("run");
            animation["run"].speed = 1f;
        }            
        else
        if (load_state == LoadState.RUNNING)
        {
            load_state = LoadState.IDLE;
            animation.Stop();
        }
            
    }
}








