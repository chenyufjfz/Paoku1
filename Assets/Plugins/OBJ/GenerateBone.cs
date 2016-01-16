using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class GenerateBone {
    public const int HIP = 0; //HIP is the centor of body, it is at waist
    public const int RIGHTUP_LEG = 1; //RIGHTUP_LEG PinYin is You Da Tui Gen bu
    public const int LEFTUP_LEG = 2; //LEFTUP_LEG is Zuo Da Tui Gen bu
    public const int RIGHT_LEG = 3; //RIGHT_LEG is You Xi gai
    public const int LEFT_LEG = 4; //LEFT_LEG is Zuo Xi gai
    public const int SPINE = 5; //SPINE is Du Qi Wang Shang
    public const int RIGHT_ARM = 6; //RIGHT_ARM is You Jian
    public const int LEFT_ARM = 7; //LEFT_ARM is Zuo Jian
    public const int RIGHTFORE_ARM = 8; //RIGHTFORE_ARM is You Zhou Guan Jie
    public const int LEFTFORE_ARM = 9; //LEFTFORE_ARM is Zuo Zhou Guan Jie
    public const int NECK = 10; //Neck is Bo Zi
    public const int RIGHT_FOOT = 11; //RIGHT_FOOT is You Jiao Huai guan Jie
    public const int LEFT_FOOT = 12; //LEFT_FOOT is Zuo Jiao Huai guan Jie
    public const int RIGHT_HAND = 13; //RIGHT_HAND is You Shou Wan guan Jie
    public const int LEFT_HAND = 14; //LEFT_HAND is Zuo Shou Wan guan Jie
    public const int VALID_PART = 11;
    public const int TOTAL_PART = 15;
    public static readonly string[] BoneName = new string[] {
        "hip",
        "rightup_leg",
        "leftup_leg",
        "right_leg",
        "left_leg",
        "spine",
        "right_arm",
        "left_arm",
        "rightfore_arm",
        "leftfore_arm",
        "neck",
        "right_foot",
        "left_foot",
        "right_hand",
        "left_hand"
    };
    public static readonly string[] RelativeBoneName = new string[] {
        "body_base/hip",
        "body_base/hip/rightup_leg",
        "body_base/hip/leftup_leg",
        "body_base/hip/rightup_leg/right_leg",
        "body_base/hip/leftup_leg/left_leg",
        "body_base/hip/spine",
        "body_base/hip/spine/right_arm",
        "body_base/hip/spine/left_arm",
        "body_base/hip/spine/right_arm/rightfore_arm",
        "body_base/hip/spine/left_arm/leftfore_arm",
        "body_base/hip/spine/neck",
        "right_foot",
        "left_foot",
        "right_hand",
        "left_hand"
    };

    /*
     * from_to return a rotation that rotats f0 to t0, and f1 to t1
     * input: f0, f1, t0, t1
     * return: Rotation
     */
    public static Quaternion from_to(Vector3 f0, Vector3 f1, Vector3 t0, Vector3 t1)
    {
        Quaternion r0 = Quaternion.FromToRotation(f0, t0);
        Vector3 f2 = r0 * f1;
        float a = Vector3.Angle(Vector3.Cross(f2, t0), Vector3.Cross(t1, t0));
        Quaternion ret = Quaternion.AngleAxis(a, t0) * r0;
        if ((ret * f0 - t0).magnitude + (ret * f1 - t1).magnitude > 0.01f)
        {
            ret = Quaternion.AngleAxis(-a, t0) * r0;
            if ((ret * f0 - t0).magnitude + (ret * f1 - t1).magnitude > 0.01f)
                Debug.Log(f0 + "," + f1 + "," + " r" + a + " -> " + ret * f0 + "," + ret * f1 + "," + Quaternion.AngleAxis(a, t0) * f2 + "=" + t0 + "," + t1);
        }

        return ret;
    }

    /*
     * from_to_zx return a rotation that rotates around z Axis first and then rotates x Axis. from f0 to t0
     * input: f0, t0
     * return: Rotation with eulerAngles.y=0
     */
    public static Quaternion from_to_zx(Vector3 f0, Vector3 t0)
    {
        Quaternion ret = Quaternion.identity;
        Vector3 f1;

        if (Mathf.Abs(f0.z) > float.Epsilon)
            throw new Exception("from_to_zx vector f0.z=" + f0.z + ",but it should be 0");
                
        if (Mathf.Abs(t0.y) < float.Epsilon && Mathf.Abs(t0.x) < float.Epsilon) 
        {
            f1 = new Vector3(0, f0.magnitude, 0);
            ret.eulerAngles = new Vector3(90 * Mathf.Sign(t0.z), 0, 90 - Mathf.Atan2(f0.y, f0.x) * 180 / Mathf.PI);
        }            
        else
        {
            ret.eulerAngles = new Vector3(-Mathf.Atan2(t0.z, t0.y) * 180 / Mathf.PI, 0, 0);
            f1 = ret * t0;
            ret.eulerAngles = new Vector3(Mathf.Atan2(t0.z, t0.y) * 180 / Mathf.PI, 0,
            (Mathf.Atan2(f1.y, f1.x) - Mathf.Atan2(f0.y, f0.x)) * 180 / Mathf.PI);
        }
            
        if ((ret * f0 - t0).magnitude > 0.01f)
        {
            Debug.Log(f0 + "->" + f1 + "->" + ret * f0 + "=" + t0 + "," + Mathf.Atan2(t0.z, t0.y).ToString() +
                 "," + (Mathf.Atan2(f1.y, f1.x) - Mathf.Atan2(f0.y, f0.x)).ToString());
        }
        return ret;
    }

    /* 
     * Compute one 3D-line which fits input points
     * Input: v set of 3D points
     * Return: line equation
     * Equation (x-bx)/kx = (y-by)/ky = z
     * x = z*kx + bx
     * y = z*ky + by
     * Return x=kx, y=ky, z=bx, w=by
     */
    public static Vector4 fit_line(Vector3 [] v)
    {
        if (v.Length < 2)
            throw new Exception("internal error: fitline v is 1");
        Matrix a= new Matrix(v.Length*2, 4);
        Matrix b= new Matrix(v.Length*2, 1);
        for (int i=0; i<v.Length; i++) {
            a[i * 2, 0] = v[i].z;
            a[i * 2, 1] = 0;
            a[i * 2, 2] = 1;
            a[i * 2, 3] = 0;
            a[i * 2 + 1, 0] = 0;
            a[i * 2 + 1, 1] = v[i].z;
            a[i * 2 + 1, 2] = 0;
            a[i * 2 + 1, 3] = 1;
            b[i * 2, 0] = v[i].x;
            b[i * 2 + 1, 0] = v[i].y;
        }

        Matrix x = a.LsmFitting(b);
        return new Vector4((float)x[0,0], (float)x[1,0], (float)x[2,0], (float)x[3,0]);
    }

    //Compute fitting point for xy plane
    //Input: v Set of 3D points
    //       z, xy plane height
    //Return: Fitting 3D points at the plane
    //compute fitting line with LMS fitline first, and then compute cross point of fitting line and z-height plane(xy)
    public static Vector3 find_fitting_point(Vector3 [] v, float z)
    {
        Vector4 b = fit_line(v);
        return new Vector3(z * b.x + b.z, z * b.y + b.w, z);
    }

    //Compute a normal postion and rotation for any pose 
    //Input: position, is joint(guan jie) position of a special pose
    //Output: normal_pos, is joint position in x-y plane
    //        normal_rot, is rotation to rotate normal_pos to position.
    //normal_pos is in x-y plane
    public static void compute_normal(Vector3 [] position, out Vector3 [] normal_pos, out Quaternion [] normal_rot)
    {
        Vector3 f0, f1, t0, t1;
        float a;

        normal_pos = new Vector3[TOTAL_PART];
        normal_rot = new Quaternion[TOTAL_PART];

        normal_pos[HIP] = position[HIP];

        //compute HIP
        t0 = position[RIGHTUP_LEG] - position[HIP];
        t1 = position[LEFTUP_LEG] - position[HIP];
        a = Mathf.Abs(Vector3.Angle(t0, t1)) /180 * Mathf.PI;
        f0 = new Vector3(-t0.magnitude * Mathf.Sin(a / 2), -t0.magnitude * Mathf.Cos(a / 2), 0);
        f1 = new Vector3(t1.magnitude * Mathf.Sin(a / 2), -t1.magnitude * Mathf.Cos(a / 2), 0);
        normal_pos[RIGHTUP_LEG] = f0;
        normal_pos[LEFTUP_LEG] = f1;
        normal_rot[HIP] = from_to(f0, f1, t0, t1);

        //Compute UP LEG
        t0 = Quaternion.Inverse(normal_rot[HIP]) * (position[RIGHT_LEG] - position[RIGHTUP_LEG]);
        f0 = new Vector3(0, -t0.magnitude, 0);
        normal_rot[RIGHTUP_LEG] = from_to_zx(f0, t0);
        normal_pos[RIGHT_LEG] = f0;

        t0 = Quaternion.Inverse(normal_rot[HIP]) * (position[LEFT_LEG] - position[LEFTUP_LEG]);
        f0 = new Vector3(0, -t0.magnitude, 0);
        normal_rot[LEFTUP_LEG] = from_to_zx(f0, t0);
        normal_pos[LEFT_LEG] = f0;

        //Compute LEG
        t0 = Quaternion.Inverse(normal_rot[RIGHTUP_LEG]) * Quaternion.Inverse(normal_rot[HIP]) * (position[RIGHT_FOOT] - position[RIGHT_LEG]);
        f0 = new Vector3(0, -t0.magnitude, 0);
        normal_rot[RIGHT_LEG] = from_to_zx(f0, t0);
        normal_pos[RIGHT_FOOT] = f0;
        normal_rot[RIGHT_FOOT] = Quaternion.identity;

        t0 = Quaternion.Inverse(normal_rot[LEFTUP_LEG]) * Quaternion.Inverse(normal_rot[HIP]) * (position[LEFT_FOOT] - position[LEFT_LEG]);
        f0 = new Vector3(0, -t0.magnitude, 0);
        normal_rot[LEFT_LEG] = from_to_zx(f0, t0);
        normal_pos[LEFT_FOOT] = f0;
        normal_rot[LEFT_FOOT] = Quaternion.identity;

        normal_pos[SPINE] = Quaternion.Inverse(normal_rot[HIP]) * (position[SPINE] - position[HIP]);

        //Compute SPINE
        t0 = Quaternion.Inverse(normal_rot[HIP]) * (position[RIGHT_ARM] - position[SPINE]);
        t1 = Quaternion.Inverse(normal_rot[HIP]) * (position[LEFT_ARM] - position[SPINE]);
        a = Mathf.Abs(Vector3.Angle(t0, t1)) / 180 * Mathf.PI;
        f0 = new Vector3(-t0.magnitude * Mathf.Sin(a / 2), t0.magnitude * Mathf.Cos(a / 2), 0);
        f1 = new Vector3(t1.magnitude * Mathf.Sin(a / 2), t1.magnitude * Mathf.Cos(a / 2), 0);
        normal_pos[RIGHT_ARM] = f0;
        normal_pos[LEFT_ARM] = f1;
        normal_rot[SPINE] = from_to(f0, f1, t0, t1);

        //Compute ARM
        t0 = Quaternion.Inverse(normal_rot[SPINE]) * Quaternion.Inverse(normal_rot[HIP]) * (position[RIGHTFORE_ARM] - position[RIGHT_ARM]);
        f0 = new Vector3(0, -t0.magnitude, 0);
        normal_rot[RIGHT_ARM] = from_to_zx(f0, t0);
        normal_pos[RIGHTFORE_ARM] = f0;

        t0 = Quaternion.Inverse(normal_rot[SPINE]) * Quaternion.Inverse(normal_rot[HIP]) * (position[LEFTFORE_ARM] - position[LEFT_ARM]);
        f0 = new Vector3(0, -t0.magnitude, 0);
        normal_rot[LEFT_ARM] = from_to_zx(f0, t0);
        normal_pos[LEFTFORE_ARM] = f0;

        //Compute fore ARM
        t0 = Quaternion.Inverse(normal_rot[RIGHT_ARM]) * Quaternion.Inverse(normal_rot[SPINE]) * Quaternion.Inverse(normal_rot[HIP]) * (position[RIGHT_HAND] - position[RIGHTFORE_ARM]);
        f0 = new Vector3(0, -t0.magnitude, 0);
        normal_rot[RIGHTFORE_ARM] = from_to_zx(f0, t0);
        normal_pos[RIGHT_HAND] = f0;
        normal_rot[RIGHT_HAND] = Quaternion.identity;

        t0 = Quaternion.Inverse(normal_rot[LEFT_ARM]) * Quaternion.Inverse(normal_rot[SPINE]) * Quaternion.Inverse(normal_rot[HIP]) * (position[LEFT_HAND] - position[LEFTFORE_ARM]);
        f0 = new Vector3(0, -t0.magnitude, 0);
        normal_rot[LEFTFORE_ARM] = from_to_zx(f0, t0);
        normal_pos[LEFT_HAND] = f0;
        normal_rot[LEFT_HAND] = Quaternion.identity;

        //Compute Neck
        normal_pos[NECK] = Quaternion.Inverse(normal_rot[SPINE]) * Quaternion.Inverse(normal_rot[HIP]) * (position[NECK] - position[SPINE]);
        normal_rot[NECK] = Quaternion.identity; 
    }

    //apply_posture is to rotate body to a special pose, now it is replaced by a Animation Curve, and not be used anymore
    //Input: rot, rotation for every joint
    //Inout: body, Transform before and after rotation
    public static void apply_posture(Quaternion[] rot, Transform[] body)
    {
        body[GenerateBone.HIP].localRotation = rot[GenerateBone.HIP];
        body[GenerateBone.RIGHTUP_LEG].localRotation = rot[GenerateBone.RIGHTUP_LEG];
        body[GenerateBone.RIGHT_LEG].localRotation = rot[GenerateBone.RIGHT_LEG];
        body[GenerateBone.LEFTUP_LEG].localRotation = rot[GenerateBone.LEFTUP_LEG];
        body[GenerateBone.LEFT_LEG].localRotation = rot[GenerateBone.LEFT_LEG];
        body[GenerateBone.SPINE].localRotation = rot[GenerateBone.SPINE];
        body[GenerateBone.RIGHT_ARM].localRotation = rot[GenerateBone.RIGHT_ARM];
        body[GenerateBone.LEFT_ARM].localRotation = rot[GenerateBone.LEFT_ARM];
        body[GenerateBone.RIGHTFORE_ARM].localRotation = rot[GenerateBone.RIGHTFORE_ARM];
        body[GenerateBone.LEFTFORE_ARM].localRotation = rot[GenerateBone.LEFTFORE_ARM];
        body[GenerateBone.NECK].localRotation = rot[GenerateBone.NECK];
    }

    //it generate bone for normal pose
    //Input: pos, normal_pos return by compute_normal
    //       scale, it is 1
    //       generate_spheer, normally it is false, for debug, it is true
    //Output:
    //       body, bone
    public static GameObject generate_bone(Vector3 [] pos, float scale, out Transform[] body, bool generate_spheer=false)
    {
        body = new Transform[TOTAL_PART];
        GameObject hip, body_base;

        body_base = new GameObject();
        body_base.name = "body_base";
        body_base.transform.localPosition = new Vector3(0, 0, 0);
        body_base.transform.localRotation = Quaternion.identity;
        body_base.transform.localScale = new Vector3(1, 1, 1);

        if (generate_spheer)
            hip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        else
            hip = new GameObject();
        body[GenerateBone.HIP] = hip.transform;
        body[GenerateBone.HIP].name = BoneName[GenerateBone.HIP];
        body[GenerateBone.HIP].parent = body_base.transform;
        body[GenerateBone.HIP].localPosition = pos[GenerateBone.HIP];
        body[GenerateBone.HIP].localRotation = Quaternion.identity;
        body[GenerateBone.HIP].localScale = new Vector3(scale, scale, scale);

        if (generate_spheer)
            body[GenerateBone.RIGHTUP_LEG] = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        else
            body[GenerateBone.RIGHTUP_LEG] = (new GameObject()).transform;
        body[GenerateBone.RIGHTUP_LEG].name = BoneName[GenerateBone.RIGHTUP_LEG];
        body[GenerateBone.RIGHTUP_LEG].parent = body[GenerateBone.HIP];
        body[GenerateBone.RIGHTUP_LEG].localPosition = pos[GenerateBone.RIGHTUP_LEG] / scale;
        body[GenerateBone.RIGHTUP_LEG].localRotation = Quaternion.identity;
        body[GenerateBone.RIGHTUP_LEG].localScale = new Vector3(1, 1, 1);

        if (generate_spheer)
            body[GenerateBone.LEFTUP_LEG] = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        else
            body[GenerateBone.LEFTUP_LEG] = (new GameObject()).transform;
        body[GenerateBone.LEFTUP_LEG].name = BoneName[GenerateBone.LEFTUP_LEG];
        body[GenerateBone.LEFTUP_LEG].parent = body[GenerateBone.HIP];
        body[GenerateBone.LEFTUP_LEG].localPosition = pos[GenerateBone.LEFTUP_LEG] /scale;
        body[GenerateBone.LEFTUP_LEG].localRotation = Quaternion.identity;
        body[GenerateBone.LEFTUP_LEG].localScale = new Vector3(1, 1, 1);

        if (generate_spheer)
            body[GenerateBone.RIGHT_LEG] = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        else
            body[GenerateBone.RIGHT_LEG] = (new GameObject()).transform;
        body[GenerateBone.RIGHT_LEG].name = BoneName[GenerateBone.RIGHT_LEG];
        body[GenerateBone.RIGHT_LEG].parent = body[GenerateBone.RIGHTUP_LEG];
        body[GenerateBone.RIGHT_LEG].localPosition = pos[GenerateBone.RIGHT_LEG] / scale;
        body[GenerateBone.RIGHT_LEG].localRotation = Quaternion.identity;
        body[GenerateBone.RIGHT_LEG].localScale = new Vector3(1, 1, 1);

        if (generate_spheer)
            body[GenerateBone.LEFT_LEG] = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        else
            body[GenerateBone.LEFT_LEG] = (new GameObject()).transform;
        body[GenerateBone.LEFT_LEG].name = BoneName[GenerateBone.LEFT_LEG];
        body[GenerateBone.LEFT_LEG].parent = body[GenerateBone.LEFTUP_LEG];
        body[GenerateBone.LEFT_LEG].localPosition = pos[GenerateBone.LEFT_LEG] / scale;
        body[GenerateBone.LEFT_LEG].localRotation = Quaternion.identity;
        body[GenerateBone.LEFT_LEG].localScale = new Vector3(1, 1, 1);

        if (generate_spheer)
            body[GenerateBone.RIGHT_FOOT] = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        else
            body[GenerateBone.RIGHT_FOOT] = (new GameObject()).transform;
        body[GenerateBone.RIGHT_FOOT].name = BoneName[GenerateBone.RIGHT_FOOT];
        body[GenerateBone.RIGHT_FOOT].parent = body[GenerateBone.RIGHT_LEG];
        body[GenerateBone.RIGHT_FOOT].localPosition = pos[GenerateBone.RIGHT_FOOT] / scale;
        body[GenerateBone.RIGHT_FOOT].localRotation = Quaternion.identity;
        body[GenerateBone.RIGHT_FOOT].localScale = new Vector3(1, 1, 1);

        if (generate_spheer)
            body[GenerateBone.LEFT_FOOT] = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        else
            body[GenerateBone.LEFT_FOOT] = (new GameObject()).transform;
        body[GenerateBone.LEFT_FOOT].name = BoneName[GenerateBone.LEFT_FOOT];
        body[GenerateBone.LEFT_FOOT].parent = body[GenerateBone.LEFT_LEG];
        body[GenerateBone.LEFT_FOOT].localPosition = pos[GenerateBone.LEFT_FOOT] / scale;
        body[GenerateBone.LEFT_FOOT].localRotation = Quaternion.identity;
        body[GenerateBone.LEFT_FOOT].localScale = new Vector3(1, 1, 1);

        if (generate_spheer)
            body[GenerateBone.SPINE] = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        else
            body[GenerateBone.SPINE] = (new GameObject()).transform;
        body[GenerateBone.SPINE].name = BoneName[GenerateBone.SPINE];
        body[GenerateBone.SPINE].parent = body[GenerateBone.HIP];
        body[GenerateBone.SPINE].localPosition = pos[GenerateBone.SPINE] / scale;
        body[GenerateBone.SPINE].localRotation = Quaternion.identity;
        body[GenerateBone.SPINE].localScale = new Vector3(1, 1, 1);

        if (generate_spheer)
            body[GenerateBone.RIGHT_ARM] = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        else
            body[GenerateBone.RIGHT_ARM] = (new GameObject()).transform;
        body[GenerateBone.RIGHT_ARM].name = BoneName[GenerateBone.RIGHT_ARM];
        body[GenerateBone.RIGHT_ARM].parent = body[GenerateBone.SPINE];
        body[GenerateBone.RIGHT_ARM].localPosition = pos[GenerateBone.RIGHT_ARM] / scale;
        body[GenerateBone.RIGHT_ARM].localRotation = Quaternion.identity;
        body[GenerateBone.RIGHT_ARM].localScale = new Vector3(1, 1, 1);

        if (generate_spheer)
            body[GenerateBone.LEFT_ARM] = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        else
            body[GenerateBone.LEFT_ARM] = (new GameObject()).transform;
        body[GenerateBone.LEFT_ARM].name = BoneName[GenerateBone.LEFT_ARM];
        body[GenerateBone.LEFT_ARM].parent = body[GenerateBone.SPINE];
        body[GenerateBone.LEFT_ARM].localPosition = pos[GenerateBone.LEFT_ARM] / scale;
        body[GenerateBone.LEFT_ARM].localRotation = Quaternion.identity;
        body[GenerateBone.LEFT_ARM].localScale = new Vector3(1, 1, 1);

        if (generate_spheer)
            body[GenerateBone.RIGHTFORE_ARM] = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        else
            body[GenerateBone.RIGHTFORE_ARM] = (new GameObject()).transform;
        body[GenerateBone.RIGHTFORE_ARM].name = BoneName[GenerateBone.RIGHTFORE_ARM];
        body[GenerateBone.RIGHTFORE_ARM].parent = body[GenerateBone.RIGHT_ARM];
        body[GenerateBone.RIGHTFORE_ARM].localPosition = pos[GenerateBone.RIGHTFORE_ARM] /scale;
        body[GenerateBone.RIGHTFORE_ARM].localRotation = Quaternion.identity;
        body[GenerateBone.RIGHTFORE_ARM].localScale = new Vector3(1, 1, 1);

        if (generate_spheer)
            body[GenerateBone.LEFTFORE_ARM] = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        else
            body[GenerateBone.LEFTFORE_ARM] = (new GameObject()).transform;
        body[GenerateBone.LEFTFORE_ARM].name = BoneName[GenerateBone.LEFTFORE_ARM];
        body[GenerateBone.LEFTFORE_ARM].parent = body[GenerateBone.LEFT_ARM];
        body[GenerateBone.LEFTFORE_ARM].localPosition = pos[GenerateBone.LEFTFORE_ARM] / scale;
        body[GenerateBone.LEFTFORE_ARM].localRotation = Quaternion.identity;
        body[GenerateBone.LEFTFORE_ARM].localScale = new Vector3(1, 1, 1);

        if (generate_spheer)
            body[GenerateBone.RIGHT_HAND] = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        else
            body[GenerateBone.RIGHT_HAND] = (new GameObject()).transform;
        body[GenerateBone.RIGHT_HAND].name = BoneName[GenerateBone.RIGHT_HAND];
        body[GenerateBone.RIGHT_HAND].parent = body[GenerateBone.RIGHTFORE_ARM];
        body[GenerateBone.RIGHT_HAND].localPosition = pos[GenerateBone.RIGHT_HAND] / scale;
        body[GenerateBone.RIGHT_HAND].localRotation = Quaternion.identity;
        body[GenerateBone.RIGHT_HAND].localScale = new Vector3(1, 1, 1);

        if (generate_spheer)
            body[GenerateBone.LEFT_HAND] = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        else
            body[GenerateBone.LEFT_HAND] = (new GameObject()).transform;
        body[GenerateBone.LEFT_HAND].name = BoneName[GenerateBone.LEFT_HAND];
        body[GenerateBone.LEFT_HAND].parent = body[GenerateBone.LEFTFORE_ARM];
        body[GenerateBone.LEFT_HAND].localPosition = pos[GenerateBone.LEFT_HAND] / scale;
        body[GenerateBone.LEFT_HAND].localRotation = Quaternion.identity;
        body[GenerateBone.LEFT_HAND].localScale = new Vector3(1, 1, 1);

        if (generate_spheer)
            body[GenerateBone.NECK] = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        else
            body[GenerateBone.NECK] = (new GameObject()).transform;
        body[GenerateBone.NECK].name = BoneName[GenerateBone.NECK];
        body[GenerateBone.NECK].parent = body[GenerateBone.SPINE];
        body[GenerateBone.NECK].localPosition = pos[GenerateBone.NECK] / scale;
        body[GenerateBone.NECK].localRotation = Quaternion.identity;
        body[GenerateBone.NECK].localScale = new Vector3(1, 1, 1);
        return hip;
    }    
}
