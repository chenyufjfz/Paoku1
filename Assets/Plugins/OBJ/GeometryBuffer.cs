using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
#if STANDALONE_DEBUG
using Vectrosity;
#endif
using NodeIdx = System.UInt16;
public struct FaceIndices
{
    public NodeIdx vi; //3D point index, it is in OBJ file
    public NodeIdx vp; //3D point and 2D uv point combine to a pair vp, it is assigned based on different (vi,vu)
    public NodeIdx vu; //2D uv point, it is in OBJ file
    public NodeIdx vn; //it is in OBJ file, normally not used
}

//VHashNode is a hash table with key vi, and content vi, vu, vp
public struct VHashNode
{
    public NodeIdx vi, vu, vp; //it equal to FaceIndices's vi, vu, vp
    public NodeIdx next_hash_idx; 
    public NodeIdx next_vi; //it uses for 2 purposes
    //1 links all VHashNode which belong to same body part (LEG0, LEG1, ARM0, ARM1)
    //2 links body part cross-section
    public byte flag; // Identify if it is local min, IS_MIN or IS_NORMAL
    public byte part; //LEG0, LEG1, ARM0 or ARM1
}

//VConnect only save vi higher than self, if v1, v2, v3 make a face and v1.z < v2.z && v1.z< v3.z
//then v2 and v3 is in vcon[v1]
public struct VConnect
{    
    public NodeIdx[] vi;  //it is FaceIndices.vi 
}

/*
 * Part is body part (LEG0, LEG1, ARM0, ARM1)
 */
public struct Part
{
    public NodeIdx head, tail; //head, tail and VHashNode.next_vi forms a link list for same body part
    public float connect_high; //it equals to lowest point which connect LEG1, ARM0, ARM1 to LEG0
    public float[] xmin, xmin_pp; //They are cross-section rectangle's xmin for different parts
    public float[] xmax, xmax_pp; //They are cross-section rectangle's xmax for different parts
    public float[] ymin, ymin_pp; //They are cross-section rectangle's ymin for different parts
    public float[] ymax, ymax_pp; //They are cross-section rectangle's ymax for different parts
    public Vector3[] center; //They are cross-section rectangle's center for different parts
    public NodeIdx[] layer_head; //layer_head and VHashNode.next_vi forms a link list for body part cross-section
#if STANDALONE_DEBUG
    public VectorLine center_line; //draw line to connect cross-section center points, only for debug
#endif
}

public class VerticesTopo
{    
    public const int VPAIR_SIZE = 65535; //Unity's limit, don't change
    public const float new_part_threshold = 7; //Tuning paramter for new part height, don't change
    public const int leg_threshold = 9; //Tuning paramter for leg upper foot, don't change
    public const NodeIdx INVALID_V = 65535; //don't change
    public const int VCONNECT_SIZE = 4; //Tuning for speed and memory cost, the bigger cost more memory cost, should >=3 and <8
    public const float HIGHT_KNEE = 0.26f; //Tuning for knee height
    public const float HIGHT_HAMROOT = 0.435f; //Tuning for leg ham height
    public const float HIGHT_ARMROOT = 0.785f; //Tuning for arm root height
    public const float HIGHT_NECK = 0.85f; //Tuning for neck height
    public const float LEN_FOREARM = 0.21f; //Tuning for forearm length
    public const float HIGHT_HIP = 0.510f; //Tuning for HIP height
    public const float HIGHT_SPINE = 0.600f; //Tuning for SPINE height
    public int SECTION_NUM = 50; //Tuning parameter for section, it depends on model's total points, should >20 and <=90, 
    //if it is small, compute_joint may fail, if it is big and point number is low, compute_joint may be not accurate
    //if total point are less than 2000, suggest 20
    //if total point are more than 10000, suggest 50
    public VHashNode[] vpair; //The major data struct for body points,vpair have two parts, the top part and bottom part, see add_hash_node
    //the top part index is vi, it map to vi; the bottom part is for same vi, different vu 
    public VConnect[] vcon; //The connection between vpair upper part, see add_connect
    public Part[] part;
    public List<Vector3> v;
    public Vector3[] joint_pos; //it is used for knee, leg, arm guanjie
    public NodeIdx vpair_count, vpair_cap, vcon_cap; //vpair_count is (vi,vu) number, vpair_cap is top and bottom part boundary
    public float total_high;
    private const byte IS_MIN = 1; //IS_MIN means local minimum point, if one point's connecting points are all higher than itself, it is marked IS_MIN
    private const byte IS_NORMAL = 0;

    public const byte UNKNOW = 4;
    public const byte LEG0 = 0; //LEG0 include one leg and body and head
    public const byte LEG1 = 1; //LEG1 is another leg
    public const byte ARM0 = 2; //ARM0 is one hand
    public const byte ARM1 = 3; //ARM1 is another hand
    public const byte TO_BE_DECIDED = 5;
    public bool leg0_right, arm0_right; //if LEG0 is right, leg0_right = ture, see judge_left_right
    public float gnd_high; //the lowest point's height

    public VerticesTopo(List <Vector3> vertices, int size = VPAIR_SIZE)
    {
        vpair = new VHashNode[size];
        vcon = new VConnect[size];
        vpair_count = 0;
        vpair_cap = 65535;
        vcon_cap = 0;
        v = vertices;
        for (int i = 0; i < vpair.Length; i++)
        {
            vpair[i].vu = INVALID_V;            
            vpair[i].next_hash_idx = INVALID_V;
            vpair[i].flag = IS_MIN;
            vpair[i].part = UNKNOW;
            vpair[i].next_vi = INVALID_V;
        }

        for (int i = 0; i < vcon.Length; i++)        
            vcon[i].vi = null;

        part = new Part[4];           
    }

    public void add_hash_node(ref FaceIndices f)
    {
        if (vpair[f.vi].vu == INVALID_V) //new vi
        {
            f.vp = vpair_count;
            vpair[f.vi].vi = f.vi;
            vpair[f.vi].vu = f.vu;
            vpair[f.vi].vp = vpair_count++;
        }
        else
        {
            int idx = f.vi;
            while (vpair[idx].vu != f.vu && vpair[idx].next_hash_idx != INVALID_V)
                idx = vpair[idx].next_hash_idx;
            if (vpair[idx].vu == f.vu) //find same (vi,vu) part, find existing pair
                f.vp = vpair[idx].vp;
            else
            { //same vi,different vu, new pair
                vpair_cap--;
                vpair[idx].next_hash_idx = vpair_cap;
                f.vp = vpair_count;
                vpair[vpair_cap].vi = f.vi;
                vpair[vpair_cap].vu = f.vu;
                vpair[vpair_cap].vp = vpair_count++;
            }
        }

        if (vpair_count >= VPAIR_SIZE)
            throw new Exception("Vertices can't exceed " + VPAIR_SIZE);
    }

    protected void add_connect(NodeIdx v0, NodeIdx v1)
    {
        int len;

        if (v0 == v1)
            return;
        vpair[v1].flag = IS_NORMAL;
        if (vcon[v0].vi != null)
        {
            for (len = 0; len < vcon[v0].vi.Length; len++)
                if (vcon[v0].vi[len] == v1 || vcon[v0].vi[len] == INVALID_V)
                { //connection list is not full, 
                    vcon[v0].vi[len] = v1;
                    return;
                }
        }
        else
            len = 0;

        //if connection list is empty or full, copy existing array
        NodeIdx[] newvi = new NodeIdx[len + VCONNECT_SIZE];
        if (len != 0)
        {
            Array.Copy(vcon[v0].vi, newvi, len);
            vcon_cap++;  
        }
            
        newvi[len] = v1;
        for (int i = len + 1; i < newvi.Length; i++)
            newvi[i] = INVALID_V;
        vcon[v0].vi = newvi;        
    }

    protected float high(NodeIdx v0)
    {
        return v[v0].z;
    }

    /*
     * Find minimum v0, and then add v1 and v2 to v0's connection table vcon
     */
    public void add_tri(NodeIdx v0, NodeIdx v1, NodeIdx v2)
    {
        NodeIdx swap;

        if (high(v0) > high(v1))
        {
            swap = v0;
            v0 = v1;
            v1 = swap;
        }

        if (high(v0) > high(v2))
        {
            swap = v0;
            v0 = v2;
            v2 = swap;
        }

        add_connect(v0, v1);
        add_connect(v0, v2);
    }

    /*
     * It sorts all IS_MIN vi from bottom to up
     * Return min_set, later it will be used as root for search_part
     */
    protected List<NodeIdx> sort_all_min()
    {
        List<NodeIdx> min_set = new List<NodeIdx>(20);

        for (NodeIdx i = 0; i < v.Count; i++)
            if (vpair[i].flag == IS_MIN)
                min_set.Add(i);

        min_set.Sort(delegate(NodeIdx a, NodeIdx b)
        {
            return high(a).CompareTo(high(b));
        });

        return min_set;
    }

    /*do borad first search based on vcon
     * input: root, the lowest point to start search
     * input: sign, it is always TO_BE_DECIDED
     * output: tail, the parts last point
     * output: min_high is the nearest height to each known part
     * Precondition: vcon is assigned valid value, 
     */
    protected float search_part(NodeIdx root, byte sign, out NodeIdx tail, out float [] min_high)
    {
        NodeIdx head = root;        
        min_high= new float[4];
        float ret = float.MinValue;

        for (int i = 0; i < 4; i++)
            min_high[i] = float.MaxValue;
        tail = root;
        while (head != INVALID_V)
        {
            if (vcon[head].vi!=null)
                for (int i = 0; i < vcon[head].vi.Length; i++)
                {
                    NodeIdx k = vcon[head].vi[i];
                    if (k == INVALID_V)
                        break;
                    if (vpair[k].part == UNKNOW)
                    {
                        if (vpair[k].flag == IS_MIN)
                            throw new Exception("add tri wrong"); //it should not happen
                        vpair[k].part = sign;
                        vpair[tail].next_vi = k;
                        tail = k;
                    }
                    else
                        if (vpair[k].part != sign)                        
                            min_high[vpair[k].part] = Mathf.Min(min_high[vpair[k].part], high(k));

                    ret = Mathf.Max(ret, high(k));                                                   
                }
            head = vpair[head].next_vi;
        }
        return ret;
    }

    /*
     * it use foot direction to judge right leg and right arm, it will assign leg0_right and arm0_right
     * Return 0, success, -1, -2, -3 can't judge left or right
     */
    protected int judge_left_right()
    {
        int i;
        int ls = SECTION_NUM / leg_threshold;
        Vector3 v1 = new Vector3(part[LEG0].xmin[ls] + part[LEG0].xmax[ls] - part[LEG1].xmin[ls] - part[LEG1].xmax[ls],
            part[LEG0].ymin[ls] + part[LEG0].ymax[ls] - part[LEG1].ymin[ls] - part[LEG1].ymax[ls], 0);
        Vector3 v2 = Vector3.Cross(new Vector3(0, 0, 1), v1); //body direction
        Vector3 v3 = new Vector3(part[LEG0].xmin[0] + part[LEG0].xmax[0] - part[LEG0].xmin[ls] - part[LEG0].xmax[ls],
            part[LEG0].ymin[0] + part[LEG0].ymax[0] - part[LEG0].ymin[ls] - part[LEG0].ymax[ls], 0); //LEG0 foot direction
        Vector3 v4 = new Vector3(part[LEG1].xmin[0] + part[LEG1].xmax[0] - part[LEG1].xmin[ls] - part[LEG1].xmax[ls],
            part[LEG1].ymin[0] + part[LEG1].ymax[0] - part[LEG1].ymin[ls] - part[LEG1].ymax[ls], 0); //LEG1 foot direction
        if (Vector3.Dot(v2, v3) * Vector3.Dot(v2, v4) < 0)
            return -1;

        leg0_right = (Vector3.Dot(v2, v3) < 0);

        for (i = SECTION_NUM / 3; i < SECTION_NUM; i++)
            if (part[ARM0].layer_head[i] != INVALID_V && part[ARM1].layer_head[i] != INVALID_V)
                break;
        if (i == SECTION_NUM)
            return -2;
        Vector3 v5 = new Vector3(part[ARM0].xmin[i] + part[ARM0].xmax[i] - part[ARM1].xmin[i] - part[ARM1].xmax[i],
            part[ARM0].ymin[i] + part[ARM0].ymax[i] - part[ARM1].ymin[i] - part[ARM1].ymax[i], 0);
        Vector3 v6 = Vector3.Cross(new Vector3(0, 0, 1), v5); //body direction
        if (Vector3.Dot(v6, v3) * Vector3.Dot(v6, v4) < 0)
            return -3;
        arm0_right = (Vector3.Dot(v6, v3) < 0);
        return 0;
    }

    /*
     * Search si zhi, left & right leg and arm, decide 3dpoints belong to which part, search point from bottom to up.
     * If search part height which is larger than threshold, then it is recognized as new si zhi.
     * Based on vpair and vcon, compute part[]
     */
    protected int search_all_part()
    {
        List<NodeIdx> min_set = sort_all_min();
        List<NodeIdx> min_set1 = new List<NodeIdx>();
        NodeIdx pos;
        float[] min_high;        
        byte part_num = 0;
        int loop = 0;

        gnd_high = high(min_set[0]);
        //init all part
        for (int i = 0; i < part.Length; i++)
        {            
            part[i].connect_high = float.MaxValue;
            part[i].head = INVALID_V;
            part[i].tail = INVALID_V;
            part[i].xmax = new float[SECTION_NUM];
            part[i].xmin = new float[SECTION_NUM];
            part[i].ymax = new float[SECTION_NUM];
            part[i].ymin = new float[SECTION_NUM];
            part[i].layer_head = new NodeIdx[SECTION_NUM];            
            for (int j = 0; j < SECTION_NUM; j++)
            {
                part[i].xmax[j] = float.MinValue;
                part[i].xmin[j] = float.MaxValue;
                part[i].ymax[j] = float.MinValue;
                part[i].ymin[j] = float.MaxValue;
                part[i].layer_head[j] = INVALID_V;               
            }
        }

        while (min_set.Count != 0 && loop <3)
        {
            Debug.Log("loop=" + loop + "min count=" + min_set.Count);
            for (int i = 0; i < min_set.Count; i++)
            {
                if (vpair[min_set[i]].part == UNKNOW)
                {
                    float h = search_part(min_set[i], TO_BE_DECIDED, out pos, out min_high) - high(min_set[i]);
                    float min_min_high = float.MaxValue / 2; //the minimum high attached to known part
                    byte min_part = 255; //which part it attached to 
                    //following find the nearest known part it attached to
                    for (byte j = 0; j < part_num; j++)
                        if (min_high[j] - high(min_set[i]) < min_min_high)
                        {
                            min_min_high = min_high[j] - high(min_set[i]);
                            min_part = j;
                        }

                    if (min_min_high < total_high / 10) // find part attached to known part
                    {//add list to known part
                        vpair[part[min_part].tail].next_vi = min_set[i]; 
                        part[min_part].tail = pos;
                        pos = min_set[i];
                        while (pos != INVALID_V)
                        {
                            vpair[pos].part = min_part;
                            pos = vpair[pos].next_vi;
                        }

                        switch (min_part)
                        {
                            case LEG0:
                                for (byte j = 1; j < 4; j++)
                                    part[j].connect_high = Mathf.Min(part[j].connect_high, min_high[j]);
                                break;
                            case LEG1:
                                part[LEG1].connect_high = Mathf.Min(part[LEG1].connect_high, min_high[LEG0]);
                                part[ARM0].connect_high = Mathf.Min(part[ARM0].connect_high, min_high[ARM0]);
                                part[ARM1].connect_high = Mathf.Min(part[ARM1].connect_high, min_high[ARM1]);
                                break;
                            case ARM0:
                                part[ARM0].connect_high = Mathf.Min(part[ARM0].connect_high, min_high[LEG0]);
                                part[ARM0].connect_high = Mathf.Min(part[ARM0].connect_high, min_high[LEG1]);
                                part[ARM1].connect_high = Mathf.Min(part[ARM1].connect_high, min_high[ARM1]);
                                break;
                            case ARM1:
                                part[ARM1].connect_high = Mathf.Min(part[ARM1].connect_high, min_high[LEG0]);
                                part[ARM1].connect_high = Mathf.Min(part[ARM1].connect_high, min_high[LEG1]);
                                part[ARM0].connect_high = Mathf.Min(part[ARM0].connect_high, min_high[ARM0]);
                                break;
                            default:
                                throw new Exception("Internal error, unknow part");
                        }
                    }
                    else
                    {
                        if (h > total_high / new_part_threshold) //find new part
                        {
                            if (part_num == 4)
                                return 5;
                            part[part_num].head = min_set[i];
                            part[part_num].tail = pos;
                            pos = min_set[i];
                            while (pos != INVALID_V)
                            {
                                vpair[pos].part = part_num;
                                pos = vpair[pos].next_vi;
                            }
                            switch (part_num)
                            {
                                case LEG0:
                                    break;
                                case LEG1:
                                    part[LEG1].connect_high = Mathf.Min(part[LEG1].connect_high, min_high[LEG0]);
                                    break;
                                case ARM0:
                                    part[ARM0].connect_high = Mathf.Min(part[ARM0].connect_high, min_high[LEG0]);
                                    part[ARM0].connect_high = Mathf.Min(part[ARM0].connect_high, min_high[LEG1]);
                                    break;
                                case ARM1:
                                    part[ARM1].connect_high = Mathf.Min(part[ARM1].connect_high, min_high[LEG0]);
                                    part[ARM1].connect_high = Mathf.Min(part[ARM1].connect_high, min_high[LEG1]);
                                    part[ARM0].connect_high = Mathf.Min(part[ARM0].connect_high, min_high[ARM0]);
                                    break;
                            }
                            part_num++;
                        }
                        else
                        {   //it is not long enough to form a new part and not attached to any existing part,
                            //so it belong a part which is not discovered, add it to min_set1 for re-search
                            pos = min_set[i];
                            while (pos != INVALID_V)
                            {
                                vpair[pos].part = UNKNOW;
                                pos = vpair[pos].next_vi;
                            }
                            min_set1.Add(min_set[i]);
                        }
                    }
                }
            }

            min_set = min_set1;
            min_set1 = new List<NodeIdx>();
            loop++;
        }        

        Debug.Log("Gnd=" + gnd_high +",Log1=" + part[LEG1].connect_high + ",ARM0=" + part[ARM0].connect_high + ",ARM1=" + part[ARM1].connect_high);

        if (part_num != 4)
        {
            Debug.Log("find more than 4 parts");
            return (int)part_num;
        }            

        //following code compute cross-section
        for (pos = 0; pos < v.Count; pos++)
        {
            if (vpair[pos].part == UNKNOW)
                continue;
            if (part[vpair[pos].part].connect_high < high(pos))
                vpair[pos].part = LEG0;

            int _part = vpair[pos].part;
            int layer = (int) Mathf.Floor((high(pos) - gnd_high) / total_high * SECTION_NUM);
            if (layer == SECTION_NUM)
                layer--;
            part[_part].xmin[layer] = Mathf.Min(v[pos].x, part[_part].xmin[layer]);
            part[_part].xmax[layer] = Mathf.Max(v[pos].x, part[_part].xmax[layer]);
            part[_part].ymin[layer] = Mathf.Min(v[pos].y, part[_part].ymin[layer]);
            part[_part].ymax[layer] = Mathf.Max(v[pos].y, part[_part].ymax[layer]);
            vpair[pos].next_vi = part[_part].layer_head[layer];
            part[_part].layer_head[layer] = pos;
        }

        if (part[LEG1].layer_head[2] == INVALID_V)
        {
            Debug.Log("Leg too short");
            return -1;
        }
                    
        return (int)part_num;
    }

    /*
     * Compute cross-section center point, based on part, compute center
     */
    protected void compute_center()
    {
        //do postprocessing for xmin, xmax, ymin, ymax
        for (int i = 0; i < part.Length; i++)
        {
            int j=0;
            part[i].xmax_pp = new float[SECTION_NUM];
            part[i].xmin_pp = new float[SECTION_NUM];
            part[i].ymax_pp = new float[SECTION_NUM];
            part[i].ymin_pp = new float[SECTION_NUM];
            while (part[i].layer_head[j] == INVALID_V && j < SECTION_NUM)
                j++;
            part[i].xmin_pp[j] = part[i].xmin[j];
            part[i].xmax_pp[j] = part[i].xmax[j];
            part[i].ymin_pp[j] = part[i].ymin[j];
            part[i].ymax_pp[j] = part[i].ymax[j];

            for (j++; j < SECTION_NUM-1; j++)                
            {
                part[i].xmin_pp[j] = part[i].xmin[j];
                part[i].xmax_pp[j] = part[i].xmax[j];
                part[i].ymin_pp[j] = part[i].ymin[j];
                part[i].ymax_pp[j] = part[i].ymax[j];
                                
                //it is a patch to fix missing points
                if (part[i].xmin[j] > part[i].xmin[j - 1] && part[i].xmin[j] > part[i].xmin[j + 1])                
                    part[i].xmin[j] = Mathf.Max(part[i].xmin[j - 1], part[i].xmin[j + 1]);
                
                if (part[i].xmax[j] < part[i].xmax[j - 1] && part[i].xmax[j] < part[i].xmax[j + 1]) 
                    part[i].xmax[j] = Mathf.Min(part[i].xmax[j - 1], part[i].xmax[j + 1]);
                
                if (part[i].ymin[j] > part[i].ymin[j - 1] && part[i].ymin[j] > part[i].ymin[j + 1])
                    part[i].ymin[j] = Mathf.Max(part[i].ymin[j - 1], part[i].ymin[j + 1]);
                
                if (part[i].ymax[j] < part[i].ymax[j - 1] && part[i].ymax[j] < part[i].ymax[j + 1])
                    part[i].ymax[j] = Mathf.Min(part[i].ymax[j - 1], part[i].ymax[j + 1]);
            }
            part[i].xmin_pp[j] = part[i].xmin[j];
            part[i].xmax_pp[j] = part[i].xmax[j];
            part[i].ymin_pp[j] = part[i].ymin[j];
            part[i].ymax_pp[j] = part[i].ymax[j];
        }

        for (int i = 0; i < part.Length; i++) 
        {
            part[i].center = new Vector3[SECTION_NUM];
            for (int j = 0; j < SECTION_NUM; j++)
                if (part[i].layer_head[j] != INVALID_V)
                    part[i].center[j] = new Vector3((part[i].xmin_pp[j] + part[i].xmax_pp[j]) / 2,
                        (part[i].ymin_pp[j] + part[i].ymax_pp[j]) / 2, total_high * (2 * j + 1) / (2 * SECTION_NUM) + gnd_high);
                else
                    part[i].center[j] = new Vector3(float.NaN, float.NaN, float.NaN);
        }            
    }

    /*
     * compute position for RIGHT_FOOT, RIGHT_LEG, LEFT_FOOT, LEFT_LEG, RIGHT_HAND, RIGHT_ARM, LEFT_HAND, LEFT_ARM...
     * Based on center, compute joint_pos
     */
    protected int compute_joint()
    {
        joint_pos = new Vector3[GenerateBone.TOTAL_PART];
        List<Vector3> v1 = new List<Vector3> ();
        List<Vector3> v2 = new List<Vector3> ();

        //Compute Foot and LEG 
        for (int i = 0; i < 2; i++)
        {
            v1.Clear();
            v2.Clear();
            for (int j = SECTION_NUM / leg_threshold; j < SECTION_NUM; j++)
                if (part[i].layer_head[j] != INVALID_V && part[i].center[j].z < part[LEG1].connect_high)
                {
                    float h = total_high * (2 * j + 1) / (2 * SECTION_NUM);
                    if (h < HIGHT_KNEE * total_high)
                        v1.Add(part[i].center[j]);
                    else
                        if (h < HIGHT_HAMROOT * total_high)
                            v2.Add(part[i].center[j]);
                }
            if (v1.Count < 3 || v2.Count < 2) //center points not enough, need to increase SECTION_NUM
                return -i;
            if ((i==LEG0 && leg0_right) || (i==LEG1 && !leg0_right)) {
                joint_pos[GenerateBone.RIGHT_FOOT] = GenerateBone.find_fitting_point(v1.ToArray(), gnd_high + total_high / leg_threshold);
                joint_pos[GenerateBone.RIGHT_LEG] = GenerateBone.find_fitting_point(v1.ToArray(), HIGHT_KNEE * total_high +gnd_high);
                joint_pos[GenerateBone.RIGHTUP_LEG] = GenerateBone.find_fitting_point(v2.ToArray(), HIGHT_HAMROOT * total_high + gnd_high);
            } else {
                joint_pos[GenerateBone.LEFT_FOOT] = GenerateBone.find_fitting_point(v1.ToArray(), gnd_high + total_high / leg_threshold);
                joint_pos[GenerateBone.LEFT_LEG] = GenerateBone.find_fitting_point(v1.ToArray(), HIGHT_KNEE * total_high + gnd_high);
                joint_pos[GenerateBone.LEFTUP_LEG] = GenerateBone.find_fitting_point(v2.ToArray(), HIGHT_HAMROOT * total_high + gnd_high);
            }                    
        }

        //compute hand and arm
        for (int i=2; i<4; i++)
        {
            v1.Clear();
            v2.Clear();
            int j0=0;
            float high_elbow = float.MinValue;
            while (part[i].layer_head[j0] == INVALID_V && j0 < SECTION_NUM)
                j0++;
            for (int j = j0; j < SECTION_NUM; j++)
                if (part[i].layer_head[j] != INVALID_V)
                {
                    float h = total_high * (2 * j + 1) / (2 * SECTION_NUM);
                    if (Vector3.Distance(part[i].center[j0], part[i].center[j]) < LEN_FOREARM * total_high)
                        v1.Add(part[i].center[j]);
                    else
                    {
                        if (high_elbow == float.MinValue)
                            high_elbow = (part[i].center[j].z + part[i].center[j - 1].z)/2;
                        if (h < HIGHT_ARMROOT * total_high)
                            v2.Add(part[i].center[j]);
                    }                        
                }
            if (v1.Count < 3 || v2.Count < 2)
                return -i;
            if ((i == ARM0 && arm0_right) || (i == ARM1 && !arm0_right)) {
                joint_pos[GenerateBone.RIGHT_HAND] = GenerateBone.find_fitting_point(v1.ToArray(), part[i].center[j0].z);
                joint_pos[GenerateBone.RIGHTFORE_ARM] = GenerateBone.find_fitting_point(v1.ToArray(), high_elbow);
                joint_pos[GenerateBone.RIGHT_ARM] = GenerateBone.find_fitting_point(v2.ToArray(), HIGHT_ARMROOT * total_high + gnd_high);
            }
            else
            {
                joint_pos[GenerateBone.LEFT_HAND] = GenerateBone.find_fitting_point(v1.ToArray(), part[i].center[j0].z);
                joint_pos[GenerateBone.LEFTFORE_ARM] = GenerateBone.find_fitting_point(v1.ToArray(), high_elbow);
                joint_pos[GenerateBone.LEFT_ARM] = GenerateBone.find_fitting_point(v2.ToArray(), HIGHT_ARMROOT * total_high + gnd_high);
            }
        }

        joint_pos[GenerateBone.SPINE] = (joint_pos[GenerateBone.RIGHT_ARM] + joint_pos[GenerateBone.LEFT_ARM] + 
            joint_pos[GenerateBone.LEFTUP_LEG] + joint_pos[GenerateBone.RIGHTUP_LEG]) / 4;
        joint_pos[GenerateBone.SPINE].z = HIGHT_SPINE * total_high + gnd_high;
        joint_pos[GenerateBone.HIP] = (joint_pos[GenerateBone.SPINE] + 
            joint_pos[GenerateBone.LEFTUP_LEG] + joint_pos[GenerateBone.RIGHTUP_LEG]) / 3; //Precisely it should be compute by interpolation between SPINE height and UP_LEG height, not /3
        joint_pos[GenerateBone.HIP].z = HIGHT_HIP * total_high + gnd_high;

        v1.Clear();
        v1.Add((joint_pos[GenerateBone.RIGHT_ARM] + joint_pos[GenerateBone.LEFT_ARM]) / 2);
        v1.Add(joint_pos[GenerateBone.SPINE]);
        joint_pos[GenerateBone.NECK] = GenerateBone.find_fitting_point(v1.ToArray(), HIGHT_NECK * total_high + gnd_high);
        return 0;
    }

    /*
     * Generate skin (point weight of joint), based on joint_pos, notes all points skin weight 
     * should be with neaby points, else action is ugly
     */
    protected BoneWeight[] generate_skin()
    {
        BoneWeight[] weights = new BoneWeight[vpair_count];
        Vector3 left_leg_normal, right_leg_normal, left_arm_normal, right_arm_normal, neck_normal;
        float left_leg_th, right_leg_th, left_arm_th, right_arm_th, neck_normal_th;
        float right_arm_high, left_arm_high;
        float leg_slope = 0.6f;

        //compute right leg judge plane
        Vector3 vec = joint_pos[GenerateBone.RIGHTUP_LEG] - joint_pos[GenerateBone.LEFTUP_LEG];
        vec.z = 0;
        right_leg_normal = Vector3.Slerp(vec.normalized, new Vector3(0, 0, -1), leg_slope);
        right_leg_normal = right_leg_normal.normalized;
        right_leg_th = Vector3.Dot(right_leg_normal, joint_pos[GenerateBone.RIGHTUP_LEG]);
        if (Vector3.Dot(right_leg_normal, joint_pos[GenerateBone.HIP]) < right_leg_th)
        {
            right_leg_normal = -right_leg_normal;
            right_leg_th = -right_leg_th;
        }

        //compute left leg judge plane
        left_leg_normal = Vector3.Slerp(-(vec.normalized), new Vector3(0, 0, -1), leg_slope);
        left_leg_normal = left_leg_normal.normalized;
        left_leg_th = Vector3.Dot(left_leg_normal, joint_pos[GenerateBone.LEFTUP_LEG]);
        if (Vector3.Dot(left_leg_normal, joint_pos[GenerateBone.HIP]) < left_leg_th)
        {
            left_leg_normal = -left_leg_normal;
            left_leg_th = -left_leg_th;
        }

        Debug.Log("right normal=" + right_leg_normal + "th=" + right_leg_th +",left normal=" + left_leg_normal +"th="+left_leg_th);

        //compute right arm judge plane
        vec = joint_pos[GenerateBone.RIGHT_ARM] - joint_pos[GenerateBone.LEFT_ARM];
        vec.z = 0;
        right_arm_normal = vec.normalized;
        right_arm_th = Vector3.Dot(right_arm_normal, joint_pos[GenerateBone.RIGHT_ARM]);
        if (Vector3.Dot(right_arm_normal, joint_pos[GenerateBone.LEFTFORE_ARM]) < right_arm_th)
        {
            right_arm_normal = -right_arm_normal;
            right_arm_th = -right_arm_th;
        }

        //compute left arm judge plane
        left_arm_normal = -right_arm_normal;
        left_arm_th = Vector3.Dot(left_arm_normal, joint_pos[GenerateBone.LEFT_ARM]);
        if (Vector3.Dot(left_arm_normal, joint_pos[GenerateBone.RIGHTFORE_ARM]) < left_arm_th)
        {
            left_arm_normal = -left_arm_normal;
            left_arm_th = -left_arm_th;
        }
        //compute neck judge plane
        vec = Vector3.Cross(vec, new Vector3(0, 0, 1));
        neck_normal = Vector3.Slerp(new Vector3(0, 0, 1), vec.normalized, 0.1f);
        neck_normal_th = Vector3.Dot(neck_normal, joint_pos[GenerateBone.NECK]);
        if (Vector3.Dot(neck_normal, joint_pos[GenerateBone.HIP]) > neck_normal_th)
        {
            neck_normal = -neck_normal;
            neck_normal_th = -neck_normal_th;
        }

        if (arm0_right)
        {
            right_arm_high = part[ARM0].connect_high;
            left_arm_high = part[ARM1].connect_high;
        }
        else
        {
            right_arm_high = part[ARM1].connect_high;
            left_arm_high = part[ARM0].connect_high;
        }
        for (int vi = 0; vi < v.Count; vi++)
        {
            float high = v[vi].z;
            int vp = vpair[vi].vp;
            weights[vp].weight0 = 1;
            switch (vpair[vi].part)
            {
                case VerticesTopo.LEG0:
                    if (high < joint_pos[GenerateBone.HIP].z)
                    { //LEG or HIP
                        if (high < part[VerticesTopo.LEG1].connect_high)
                        { //LEG
                            if (leg0_right)
                            {
                                if (high < joint_pos[GenerateBone.RIGHT_LEG].z)
                                    weights[vp].boneIndex0 = GenerateBone.RIGHT_LEG;
                                else
                                    weights[vp].boneIndex0 = GenerateBone.RIGHTUP_LEG;
                            }
                            else
                            {
                                if (high < joint_pos[GenerateBone.LEFT_LEG].z)
                                    weights[vp].boneIndex0 = GenerateBone.LEFT_LEG;
                                else
                                    weights[vp].boneIndex0 = GenerateBone.LEFTUP_LEG;
                            }
                        }
                        else
                        {                   
                            float rcheck = Vector3.Dot(right_leg_normal, v[vi]) - right_leg_th;
                            float lcheck = Vector3.Dot(left_leg_normal, v[vi]) - left_leg_th;
                            float rdistance = Vector3.Distance(v[vi], joint_pos[GenerateBone.RIGHTUP_LEG]);
                            float ldistance = Vector3.Distance(v[vi], joint_pos[GenerateBone.LEFTUP_LEG]);
                            float hdistance = joint_pos[GenerateBone.HIP].z - high;
                            if (rcheck > 0 && lcheck > 0)
                            {//HIP and leg border  
                                if (rdistance > ldistance)
                                {
                                    float w0 = lcheck; //distance to left leg judge plane
                                    float w1 = rdistance - ldistance; //distance to HIP midperpendicular plane
                                    float w2 = hdistance; //distance to HIP plane
                                    weights[vp].weight0 = w1 * w2 / (w0 * w1 + w0 * w2 + w1 * w2); //= (1/w0) /(1/w0 +1/w1 +1/w2)                        
                                    weights[vp].boneIndex0 = GenerateBone.LEFTUP_LEG;
                                }
                                else
                                {
                                    float w0 = rcheck;
                                    float w1 = ldistance - rdistance;
                                    float w2 = hdistance;
                                    weights[vp].weight0 = w1 * w2 / (w0 * w1 + w0 * w2 + w1 * w2);  
                                    weights[vp].boneIndex0 = GenerateBone.RIGHTUP_LEG;
                                }
                                if (weights[vp].weight0 >= 1.0)
                                    weights[vp].weight0 = 1.0f;
                                weights[vp].weight1 = 1.0f - weights[vp].weight0;
                                weights[vp].boneIndex1 = GenerateBone.HIP;
                            }
                            else
                            {//LEG
                                if (rdistance > ldistance) 
                                    weights[vp].boneIndex0 = GenerateBone.LEFTUP_LEG;                                          
                                else
                                    weights[vp].boneIndex0 = GenerateBone.RIGHTUP_LEG;
                            }                         
                        }
                    }
                    else
                    {
                        if (high < joint_pos[GenerateBone.SPINE].z)
                        { //SPINE HIP border
                            float w0 = joint_pos[GenerateBone.SPINE].z - high;
                            float w1 = high - joint_pos[GenerateBone.HIP].z;
                            weights[vp].weight0 = w1 / (w0 + w1);
                            weights[vp].weight1 = 1 - weights[vp].weight0;
                            weights[vp].boneIndex0 = GenerateBone.HIP;
                            weights[vp].boneIndex1 = GenerateBone.SPINE;
                        }
                        else
                        {
                            if (Vector3.Dot(neck_normal, v[vi]) > neck_normal_th)
                                weights[vp].boneIndex0 = GenerateBone.NECK;
                            else
                            {
                                float w0 = right_arm_th - Vector3.Dot(right_arm_normal, v[vi]); //distance to right arm judge plane
                                float w1 = v[vi].z - right_arm_high; //distance to right arm horizontal plane
                                if (w0 > 0 && w1 > 0)
                                {
                                    weights[vp].weight1 = w1 / (6 * w0);
                                    if (weights[vp].weight1 >= 1)
                                        weights[vp].weight1 = 1;
                                    weights[vp].weight0 = 1 - weights[vp].weight1;
                                    weights[vp].boneIndex0 = GenerateBone.RIGHT_ARM;
                                    weights[vp].boneIndex1 = GenerateBone.SPINE;
                                }                                    
                                else
                                {
                                    w0 = left_arm_th - Vector3.Dot(left_arm_normal, v[vi]); //distance to left arm judge plane
                                    w1 = v[vi].z - left_arm_high; //distance to left arm horizontal plane
                                    if (w0 > 0 && w1 > 0)
                                    {
                                        weights[vp].weight0 = w1 / (6 * w0);
                                        if (weights[vp].weight0 >= 1)
                                            weights[vp].weight0 = 1;
                                        weights[vp].weight1 = 1 - weights[vp].weight0;
                                        weights[vp].boneIndex0 = GenerateBone.SPINE;
                                        weights[vp].boneIndex1 = GenerateBone.LEFT_ARM;
                                    }                                        
                                    else
                                        weights[vp].boneIndex0 = GenerateBone.SPINE;
                                }
                                    
                            }
                        }
                    }

                    break;
                case VerticesTopo.LEG1:
                    if (!leg0_right)
                    {
                        if (high < joint_pos[GenerateBone.RIGHT_LEG].z)
                            weights[vp].boneIndex0 = GenerateBone.RIGHT_LEG;
                        else
                            weights[vp].boneIndex0 = GenerateBone.RIGHTUP_LEG;
                    }
                    else
                    {
                        if (high < joint_pos[GenerateBone.LEFT_LEG].z)
                            weights[vp].boneIndex0 = GenerateBone.LEFT_LEG;
                        else
                            weights[vp].boneIndex0 = GenerateBone.LEFTUP_LEG;
                    }
                    break;
                case VerticesTopo.ARM0:
                    if (arm0_right)
                    {
                        if (high < joint_pos[GenerateBone.RIGHTFORE_ARM].z)
                            weights[vp].boneIndex0 = GenerateBone.RIGHTFORE_ARM;
                        else
                            weights[vp].boneIndex0 = GenerateBone.RIGHT_ARM;
                    }
                    else
                    {
                        if (high < joint_pos[GenerateBone.RIGHTFORE_ARM].z)
                            weights[vp].boneIndex0 = GenerateBone.LEFTFORE_ARM;
                        else
                            weights[vp].boneIndex0 = GenerateBone.LEFT_ARM;
                    }
                    break;
                case VerticesTopo.ARM1:
                    if (!arm0_right)
                    {
                        if (high < joint_pos[GenerateBone.RIGHTFORE_ARM].z)
                            weights[vp].boneIndex0 = GenerateBone.RIGHTFORE_ARM;
                        else
                            weights[vp].boneIndex0 = GenerateBone.RIGHT_ARM;
                    }
                    else
                    {
                        if (high < joint_pos[GenerateBone.LEFTFORE_ARM].z)
                            weights[vp].boneIndex0 = GenerateBone.LEFTFORE_ARM;
                        else
                            weights[vp].boneIndex0 = GenerateBone.LEFT_ARM;
                    }
                    break;
            }
            int k = vpair[vi].next_hash_idx;
            while (k != INVALID_V)
            {
                weights[vpair[k].vp] = weights[vp];
                k = vpair[k].next_hash_idx;
            }
        }

        return weights;
    }

    /*
     * it is major API to build body
     * Precondition: vpair and vcon is already read.
     */
    public int build_body(out BoneWeight [] weights)
    {
        int ret;
        weights = null;
        if ((ret=search_all_part()) != 4)
        {
            Debug.Log("Error: search_all_part return" + ret);
            return -1;
        }
            
        if ((ret=judge_left_right()) != 0)
        {
            Debug.Log("Error: judge_left_right return " + ret);
            return -2;
        }
        compute_center();
        if ((ret=compute_joint()) != 0)
        {
            Debug.Log("Error: compute_joint return " + ret);
            return -3;
        }
        weights = generate_skin();
        return 0;
    }

    /*
     * it is only for debug to show joint
     */
    public GameObject generate_joint()
    {        
        GameObject parent = new GameObject("center");
        parent.transform.position = new Vector3(0,0,0);
        parent.transform.eulerAngles = new Vector3(270, 0, 0);
        parent.transform.localScale = new Vector3(1, 1, 1);

#if STANDALONE_DEBUG
        List<Vector3> line = new List<Vector3>();
        for (int i = 0; i < 4; i++)
        {
            line.Clear();
            for (int j = 0; j < SECTION_NUM; j++)
                if (part[i].layer_head[j] != INVALID_V)
                    line.Add(part[i].center[j]);
            part[i].center_line = new VectorLine("Center_line"+i, line.ToArray(), new Color(1,0,0), null, 2.0f, LineType.Continuous, Joins.Fill);
            part[i].center_line.drawTransform = parent.transform;
            part[i].center_line.Draw3D();
        }
#endif

        float scale = total_high / 50;
        GameObject joint = new GameObject("joint");
        joint.transform.localPosition = new Vector3(0, 0, 0);
        joint.transform.localScale = new Vector3(scale, scale, scale);
        joint.transform.parent = parent.transform;
        joint.transform.localRotation = Quaternion.identity;

        for (int i = 0; i < GenerateBone.TOTAL_PART; i++) 
        {
            Transform t;
            t = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            t.name = "part" +i;
            t.parent = joint.transform;
            t.localPosition = joint_pos[i] /scale;
            t.localRotation = Quaternion.identity;
            t.localScale = new Vector3(1, 1, 1);
        }
       
        return parent;
    }

    /*
     * it is only debug for show envelope
     */
    public GameObject generate_envelope(Vector3 pos)
    {
        GameObject parent = new GameObject("envelope");
        parent.transform.position = pos;
        parent.transform.eulerAngles = new Vector3(270, 0, 0);
        parent.transform.localScale = new Vector3(1, 1, 1);

        for (int i = 0; i < 4; i++)
        {
            if (i == LEG0 && leg0_right || i==LEG1 && !leg0_right)
                continue;
            if (i == ARM0 && arm0_right || i == ARM1 && !arm0_right)
                continue;
            for (int j = 0; j < SECTION_NUM; j++)
                if (part[i].layer_head[j] != INVALID_V)
                {
                    Transform t = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                    float x0 = part[i].xmin[j];
                    float x1 = part[i].xmax[j];
                    float y0 = part[i].ymin[j];
                    float y1 = part[i].ymax[j];
                    t.name = "section" + i + ":" + j;
                    t.parent = parent.transform;
                    t.localPosition = new Vector3((x0 + x1) / 2, (y0 + y1) / 2, total_high * j / SECTION_NUM + total_high / (2 * SECTION_NUM));
                    t.localRotation = Quaternion.identity;
                    t.localScale = new Vector3(x1 - x0, y1 - y0, total_high / SECTION_NUM);
                }
        }
            
        return parent;
    }
}

public class GeometryBuffer {
    public GameObject envelope, joints, bone_hip;
	private List<ObjectData> objects; //currently only 1 object support
    public List<Vector3> vertices; //its index is FaceIndices's vi
    public List<Vector2> uvs; //its index is FaceIndices's vu
	public List<Vector3> normals;
    public VerticesTopo vtopo;
    public Vector3[] normal_pos;
    public Quaternion[] normal_rot;
    private float xmin, xmax, ymin, ymax, zmin, zmax;
	private ObjectData current;
	private class ObjectData {
		public string name;
        public List<GroupData> groups; //currently only one group is support
		public List<FaceIndices> allFaces;
		public ObjectData() {
			groups = new List<GroupData>();
			allFaces = new List<FaceIndices>();
		}
	}
	
	private GroupData curgr;
	private class GroupData {
		public string name;
		public string materialName;
		public List<FaceIndices> faces;
		public GroupData() {
			faces = new List<FaceIndices>();
		}
		public bool isEmpty { get { return faces.Count == 0; } }
	}
	
	public GeometryBuffer() {
		objects = new List<ObjectData>();
		ObjectData d = new ObjectData();
		d.name = "default";
		objects.Add(d);
		current = d;
        envelope = null;
        joints = null;
		GroupData g = new GroupData();
		g.name = "default";
		d.groups.Add(g);
		curgr = g;
		
		vertices = new List<Vector3>(40960);
		uvs = new List<Vector2>(40960);
		normals = new List<Vector3>();
        vtopo = new VerticesTopo(vertices);
        
        xmin = float.MaxValue;
        xmax = float.MinValue;
        ymin = float.MaxValue;
        ymax = float.MinValue;
        zmin = float.MaxValue;
        zmax = float.MinValue;
	}

    public void Release()
    {
        if (envelope!=null)        
            UnityEngine.Object.Destroy(envelope);

        if (joints != null)
        {
            UnityEngine.Object.Destroy(joints);
#if STANDALONE_DEBUG
            for (int i = 0; i < 4; i++)
                VectorLine.Destroy(ref vtopo.part[i].center_line);
#endif
        } 
            
        
        if (bone_hip != null)        
            UnityEngine.Object.Destroy(bone_hip.transform.parent.gameObject);
        
    }

    public void PushObject(string name) {
		//Debug.Log("Adding new object " + name + ". Current is empty: " + isEmpty);
		if(isEmpty) objects.Remove(current);
		
		ObjectData n = new ObjectData();
		n.name = name;
		objects.Add(n);
		
		GroupData g = new GroupData();
		g.name = "default";
		n.groups.Add(g);
		
		curgr = g;
		current = n;
	}
	
	public void PushGroup(string name) {
		if(curgr.isEmpty) current.groups.Remove(curgr);
		GroupData g = new GroupData();
		g.name = name;
		current.groups.Add(g);
		curgr = g;
	}
	
	public void PushMaterialName(string name) {
		//Debug.Log("Pushing new material " + name + " with curgr.empty=" + curgr.isEmpty);
		if(!curgr.isEmpty) PushGroup(name);
		if(curgr.name == "default") curgr.name = name;
		curgr.materialName = name;
	}
	
	public void PushVertex(Vector3 v) {        
        xmin = Mathf.Min(xmin, v.x);
        xmax = Mathf.Max(xmax, v.x);
        ymin = Mathf.Min(ymin, v.y);
        ymax = Mathf.Max(ymax, v.y);
        zmin = Mathf.Min(zmin, v.z);
        zmax = Mathf.Max(zmax, v.z);
		vertices.Add(v);
	}
	
	public void PushUV(Vector2 v) {
		uvs.Add(v);
	}
	
	public void PushNormal(Vector3 v) {
		normals.Add(v);
	}
	
	public void PushFace(ref FaceIndices f) {
        vtopo.add_hash_node(ref f);
        
		curgr.faces.Add(f);
		current.allFaces.Add(f);        //TODO delete one Add
	}
	
	public void Trace() {
		Debug.Log("OBJ has " + objects.Count + " object(s)");
		Debug.Log("OBJ has " + vertices.Count + " vertice(s)");
		Debug.Log("OBJ has " + uvs.Count + " uv(s)");
		Debug.Log("OBJ has " + normals.Count + " normal(s)");
		foreach(ObjectData od in objects) {
			Debug.Log(od.name + " has " + od.groups.Count + " group(s)");
			foreach(GroupData gd in od.groups) {
				Debug.Log(od.name + "/" + gd.name + " has " + gd.faces.Count + " faces(s)");
			}
		}		
	}
	
	public int numObjects { get { return objects.Count; } }	
	public bool isEmpty { get { return vertices.Count == 0; } }
	public bool hasUVs { get { return uvs.Count > 0; } }
	public bool hasNormals { get { return normals.Count > 0; } }
	
    public void show_envelop(bool active)
    {
        if (envelope == null)
            envelope = vtopo.generate_envelope(new Vector3(0, (zmin-zmax)/2, 0));
        envelope.SetActive(active);
    }
    
    public void show_joint(bool active)
    {
        if (joints == null)
            joints = vtopo.generate_joint();
        joints.SetActive(active);
    }

    /*
     * This is the major GeometryBuffer API
     * Input gs[], game object for build body
     * Input mats[], material for renderer
     * Input high, game object high
     */
    public void PopulateMeshes(GameObject[] gs, Dictionary<string, Material> mats, float high =-1) {
		if(gs.Length != numObjects) return; // Should not happen unless obj file is corrupt...
		
		for(int i = 0; i < gs.Length; i++) {
			ObjectData od = objects[i];
			
			if(od.name != "default") gs[i].name = od.name;

            Vector3[] tvertices = new Vector3[vtopo.vpair_count];//its index is VHashNode.vp, its content is vector3 space points
            Vector2[] tuvs = new Vector2[vtopo.vpair_count]; //its index is VHashNode.vp, its content is vector2 uv points
            Vector3[] tnormals = new Vector3[vtopo.vpair_count]; //not used

            Vector3 vcenter = new Vector3((xmin + xmax) / 2, (ymin + ymax) / 2, (zmin + zmax) / 2);
            for (int vi = 0; vi < vertices.Count; vi++)
            {
                vertices[vi] = vertices[vi] - vcenter;//vertices is shift to origin
                
                int k = vi;
                do
                {
                    tvertices[vtopo.vpair[k].vp] = vertices[vi];                    
                    tuvs[vtopo.vpair[k].vp] = uvs[vtopo.vpair[k].vu];
                    k = vtopo.vpair[k].next_hash_idx;
                } while (k != VerticesTopo.INVALID_V);
            }
    
            Debug.Log("VCount=" + vertices.Count + ",Vpair=" + tvertices.Length + ",VpairCap=" + vtopo.vpair_cap +",Tris=" + od.allFaces.Count);
            Mesh m = new Mesh();
            m.normals = null;
            m.uv = null;
            m.triangles = null;
			m.vertices = tvertices;			
            			
			if(od.groups.Count == 1) {
				GroupData gd = od.groups[0];
#if false
                Color32[] color = new Color32[512 * 512];
                for (int y = 0; y < 512; y++)
                    for (int x = 0; x < 512; x++)
                        color[(y << 9) + x] = new Color32((byte)(y & 0xff), 0, 0, 255);
                Texture2D tex = new Texture2D(512, 512, TextureFormat.ARGB32, false);
                tex.SetPixels32(color);
                tex.Apply();  
                gs[i].renderer.material.mainTexture = tex;
#endif
				gs[i].renderer.material = mats[gd.materialName];

                if (gs[i].renderer.material==null)
                    Debug.Log("paoku load material fail");
                else
                    Debug.Log("paoku load material " + gd.materialName);

				int[] triangles = new int[gd.faces.Count];
                for (int j = 0; j < triangles.Length; j++)
                {
                    triangles[j] = gd.faces[j].vp;
                    if (j % 3 == 0)
                        vtopo.add_tri(gd.faces[j].vi, gd.faces[j + 1].vi, gd.faces[j + 2].vi); //set up connection
                }
                Debug.Log("vcon_cap=" + vtopo.vcon_cap +",zmin=" + zmin + ",zmax="+ zmax);
                vtopo.total_high = zmax - zmin;                
				m.triangles = triangles;
                if (hasUVs) m.uv = tuvs;
                if (hasNormals)
                    m.normals = tnormals;
                else
                    m.RecalculateNormals();
                      
                Transform[] bones;
                BoneWeight[] weights;
                //After point and connecttion is set up, build body
                int ret = vtopo.build_body(out weights);
                Debug.Log("build body return" + ret);
                m.boneWeights = weights;
                GenerateBone.compute_normal(vtopo.joint_pos, out normal_pos, out normal_rot);
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                bone_hip = GenerateBone.generate_bone(normal_pos, 1, out bones, true);
#else
                bone_hip = GenerateBone.generate_bone(normal_pos, 1, out bones, false);
#endif
                GenerateBone.apply_posture(normal_rot, bones);
                bones[GenerateBone.HIP].parent.parent = gs[i].transform;  //make body_base belong current object's child            
                Matrix4x4[] bindPoses = new Matrix4x4[GenerateBone.TOTAL_PART];
                for (int j = 0; j < GenerateBone.TOTAL_PART; j++)
                    bindPoses[j] = bones[j].worldToLocalMatrix * gs[i].transform.localToWorldMatrix;
                m.bindposes = bindPoses;
                (gs[i].renderer as SkinnedMeshRenderer).sharedMesh = m;                
                (gs[i].renderer as SkinnedMeshRenderer).bones = bones;
                if (high>0) //make model's high equal to high
                    bones[GenerateBone.HIP].parent.localScale = new Vector3(high / vtopo.total_high, high / vtopo.total_high, high / vtopo.total_high);
                bones[GenerateBone.HIP].parent.localPosition = new Vector3(0, 0, 0); //After it become object's child, need to readjust its local position to zero
                bones[GenerateBone.HIP].parent.eulerAngles = new Vector3(0, 270, 0); //make person face screen
                bones[GenerateBone.HIP].localEulerAngles = new Vector3(0, 0, 0);
                bones[GenerateBone.HIP].localPosition = new Vector3(0, 0, 0);
                //following is to make leg longer to let running more beautiful
                Vector3 leg_scale = bones[GenerateBone.LEFTUP_LEG].localScale;
                leg_scale.y = leg_scale.y * 1.13f;
                bones[GenerateBone.LEFTUP_LEG].localScale = leg_scale;
                leg_scale = bones[GenerateBone.RIGHTUP_LEG].localScale;
                leg_scale.y = leg_scale.y * 1.13f;
                bones[GenerateBone.RIGHTUP_LEG].localScale = leg_scale;
                leg_scale = bones[GenerateBone.LEFT_LEG].localScale;
                leg_scale.y = leg_scale.y * 1.1f;
                bones[GenerateBone.LEFT_LEG].localScale = leg_scale;
                leg_scale = bones[GenerateBone.RIGHT_LEG].localScale;
                leg_scale.y = leg_scale.y * 1.1f;
                bones[GenerateBone.RIGHT_LEG].localScale = leg_scale;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                System.IO.StreamWriter sw = new System.IO.StreamWriter("Normal.txt");
                for (int j = 0; j < GenerateBone.TOTAL_PART; j++)
                    sw.Write(j + ":" + normal_pos[j] + "," + normal_rot[j].eulerAngles + "\n");
                sw.Close();
#endif
#if false
                BoneWeight[] weights = new BoneWeight[tuvs.Length];
                for (int j = 0; j < weights.Length; j++)
                {
                    weights[j].boneIndex0 = 0;
                    weights[j].weight0 = 1;
                }
                m.boneWeights = weights;
                Transform[] bones = new Transform[1];
                Matrix4x4[] bindPoses = new Matrix4x4[1];
                bones[0] = new GameObject("hip").transform;
                bones[0].parent = gs[i].transform;
                bones[0].localRotation = Quaternion.identity;
                bones[0].localPosition = Vector3.zero;
                bindPoses[0] = bones[0].worldToLocalMatrix * gs[i].transform.localToWorldMatrix;                
                (gs[i].renderer as SkinnedMeshRenderer).sharedMesh = m;
                (gs[i].renderer as SkinnedMeshRenderer).sharedMesh.bindposes = bindPoses;
                (gs[i].renderer as SkinnedMeshRenderer).bones = bones;
#endif                
			} else {
				int gl = od.groups.Count;
				Material[] sml = new Material[gl];
				m.subMeshCount = gl;
				int c = 0;
				
				for(int j = 0; j < gl; j++) {
					sml[j] = mats[od.groups[j].materialName]; 
					int[] triangles = new int[od.groups[j].faces.Count];
					int l = od.groups[j].faces.Count + c;
					int s = 0;
					for(; c < l; c++, s++) triangles[s] = c;
					m.SetTriangles(triangles, j);
				}
				
				gs[i].renderer.materials = sml;
			}
		}
	}
}