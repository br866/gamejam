using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FormalArtSfxReview
{
    public static void Human()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat; RenderSettings.ambientLight = Color.gray * .8f;
        var light = new GameObject("Review Light").AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.5f; light.transform.rotation = Quaternion.Euler(35,-25,0);
        var go = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MoMing/FormalLevels/Prefabs/FormalHumanVisual.prefab"));
        go.transform.position = Vector3.zero; go.transform.rotation = Quaternion.identity;
        var camera = CameraFor(BoundsOf(go),new Vector3(.4f,.08f,1)); Capture(camera,"Human_front");
        camera.transform.position = BoundsOf(go).center + new Vector3(1,.05f,.1f).normalized * Mathf.Max(BoundsOf(go).size.magnitude * 1.25f,3); camera.transform.LookAt(BoundsOf(go).center); Capture(camera,"Human_side");
        EditorApplication.Exit(0);
    }
    static string Out => Environment.GetEnvironmentVariable("SFX_REVIEW_OUTPUT");
    static readonly StringBuilder inventory = new StringBuilder("scene\tpath\tmesh\tmaterials\tposition\tsize\n");
    static readonly StringBuilder monsters = new StringBuilder();
    static string PathOf(Transform t) => t.parent == null ? t.name : PathOf(t.parent) + "/" + t.name;
    static void Capture(Camera camera, string name)
    {
        var target = new RenderTexture(1200, 900, 24);var old = camera.targetTexture;var active = RenderTexture.active;
        camera.targetTexture = target;camera.Render();RenderTexture.active = target;
        var texture = new Texture2D(1200, 900, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, 1200, 900), 0, 0);texture.Apply();
        File.WriteAllBytes(System.IO.Path.Combine(Out, name + ".png"), texture.EncodeToPNG());
        camera.targetTexture = old;RenderTexture.active = active;
        UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(target);
    }
    static Bounds BoundsOf(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>().Where(r => r.enabled && !(r is ParticleSystemRenderer)).ToArray();
        var bounds = renderers.Length > 0 ? renderers[0].bounds : new Bounds(go.transform.position, Vector3.one * 2);
        foreach (var r in renderers) bounds.Encapsulate(r.bounds);return bounds;
    }
    static Camera CameraFor(Bounds bounds, Vector3 direction)
    {
        var camera = new GameObject("[SFX Art Review Camera]").AddComponent<Camera>();
        camera.fieldOfView = 40;camera.nearClipPlane = .03f;camera.farClipPlane = 500;
        camera.transform.position = bounds.center + direction.normalized * Mathf.Max(bounds.size.magnitude * 1.25f, 3f);
        camera.transform.LookAt(bounds.center);camera.clearFlags = CameraClearFlags.SolidColor;camera.backgroundColor = new Color(.09f,.095f,.10f);
        return camera;
    }
    static void DescribeMonster(MonsterPatrol m, string label)
    {
        monsters.AppendLine(label + " object=" + PathOf(m.transform) + " scale=" + m.transform.lossyScale);
        foreach (var animator in m.GetComponentsInChildren<Animator>())
        {
            monsters.AppendLine("  ACTIVE animator=" + PathOf(animator.transform) + " controller=" + AssetDatabase.GetAssetPath(animator.runtimeAnimatorController));
            foreach (var clip in animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.animationClips.Distinct() : new AnimationClip[0])
                monsters.AppendLine("    clip=" + clip.name + " duration=" + clip.length + " asset=" + AssetDatabase.GetAssetPath(clip));
        }
        foreach (var renderer in m.GetComponentsInChildren<SkinnedMeshRenderer>())
            monsters.AppendLine("  skin=" + AssetDatabase.GetAssetPath(renderer.sharedMesh) + " materials=" + string.Join(";", renderer.sharedMaterials.Select(AssetDatabase.GetAssetPath)));
    }
    public static void Run()
    {
        Directory.CreateDirectory(Out);
        foreach (string role in new[] { "MonsterA", "Monster2", "MonsterC" })
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight = Color.gray * .8f;
            var light = new GameObject("Review Key Light").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.5f;light.transform.rotation=Quaternion.Euler(35,-25,0);
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MoMing/FormalLevels/Prefabs/Monster/"+role+".prefab");
            var go=UnityEngine.Object.Instantiate(prefab);go.transform.position=Vector3.zero;go.transform.rotation=Quaternion.identity;
            DescribeMonster(go.GetComponent<MonsterPatrol>(), "PREFAB " + role);
            var camera=CameraFor(BoundsOf(go),new Vector3(.4f,.08f,1));Capture(camera,role+"_front");
            camera.transform.position=BoundsOf(go).center+new Vector3(1,.05f,.1f).normalized*Mathf.Max(BoundsOf(go).size.magnitude*1.25f,3);camera.transform.LookAt(BoundsOf(go).center);Capture(camera,role+"_side");
            var animator=go.GetComponentInChildren<Animator>();
            var clips=animator!=null && animator.runtimeAnimatorController!=null ? animator.runtimeAnimatorController.animationClips.Distinct().Where(c=>c.name.IndexOf("walk",StringComparison.OrdinalIgnoreCase)>=0).ToArray():new AnimationClip[0];
            foreach(var clip in clips.Take(2))
            {
                AnimationMode.StartAnimationMode();
                for(int i=0;i<4;i++)
                {
                    AnimationMode.BeginSampling();AnimationMode.SampleAnimationClip(animator.gameObject,clip,clip.length*i/4f);AnimationMode.EndSampling();
                    var b=BoundsOf(go);camera.transform.position=b.center+new Vector3(.7f,.08f,1).normalized*Mathf.Max(b.size.magnitude*1.25f,3);camera.transform.LookAt(b.center);
                    Capture(camera,role+"_"+clips.ToList().IndexOf(clip)+"_walk_"+i);
                }
                AnimationMode.StopAnimationMode();
            }
        }
        foreach(string scene in new[]{"FormalLevel01","FormalLevel02","FormalLevel03","FormalLevel04","FormalLevel045","FormalLevel05"})
        {
            var s=EditorSceneManager.OpenScene("Assets/MoMing/FormalLevels/"+scene+".unity",OpenSceneMode.Single);
            var renderers=s.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Renderer>()).Where(r=>r.enabled).ToArray();
            foreach(var r in renderers)
            {
                var filter=r.GetComponent<MeshFilter>();var skin=r as SkinnedMeshRenderer;
                inventory.AppendLine(scene+"\t"+PathOf(r.transform)+"\t"+AssetDatabase.GetAssetPath(filter!=null?filter.sharedMesh:skin!=null?skin.sharedMesh:null)+"\t"+string.Join(";",r.sharedMaterials.Select(AssetDatabase.GetAssetPath))+"\t"+r.bounds.center+"\t"+r.bounds.size);
            }
            int n=0;
            foreach(var m in s.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<MonsterPatrol>()))
            {
                DescribeMonster(m,scene);var c=CameraFor(BoundsOf(m.gameObject),new Vector3(.4f,.1f,1));Capture(c,scene+"_monster_"+n++);UnityEngine.Object.DestroyImmediate(c.gameObject);
            }
            var selected=renderers.Where(r=>new[]{"door","gate","bed","crate","rail","floor"}.Any(k=>r.name.IndexOf(k,StringComparison.OrdinalIgnoreCase)>=0)).GroupBy(r=>System.Text.RegularExpressions.Regex.Replace(r.name,@"[\d\s()]+","")).Select(g=>g.First()).Take(7).ToArray();
            n=0;foreach(var r in selected)
            {
                var c=CameraFor(r.bounds,new Vector3(.6f,.4f,1));Capture(c,scene+"_prop_"+n++);UnityEngine.Object.DestroyImmediate(c.gameObject);
            }
        }
        File.WriteAllText(System.IO.Path.Combine(Out,"formal-art-inventory.tsv"),inventory.ToString());
        File.WriteAllText(System.IO.Path.Combine(Out,"active-monster-visuals.txt"),monsters.ToString());
        EditorApplication.Exit(0);
    }
    class Prop { public string name; public Mesh mesh; public Material[] materials; public Vector3 scale; public Quaternion rotation; }
    public static void Details()
    {
        Directory.CreateDirectory(Out);var props=new List<Prop>();var seen=new HashSet<string>();
        foreach(string scene in new[]{"FormalLevel01","FormalLevel02","FormalLevel03","FormalLevel04","FormalLevel045","FormalLevel05"})
        {
            var s=EditorSceneManager.OpenScene("Assets/MoMing/FormalLevels/"+scene+".unity",OpenSceneMode.Single);
            foreach(var r in s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<MeshRenderer>()))
            {
                var mf=r.GetComponent<MeshFilter>();if(mf==null || mf.sharedMesh==null)continue;
                string key=string.Join(";",r.sharedMaterials.Select(AssetDatabase.GetAssetPath));
                if(!new[]{"floor","door","Pedal","bed","pipe","grille","crate","window","lamp"}.Any(k=>key.IndexOf(k,StringComparison.OrdinalIgnoreCase)>=0)||!seen.Add(key))continue;
                props.Add(new Prop{name=r.name,mesh=mf.sharedMesh,materials=r.sharedMaterials,scale=r.transform.lossyScale,rotation=r.transform.rotation});
            }
        }
        var index=new StringBuilder();int number=0;
        foreach(var prop in props)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=Color.gray;
            var light=new GameObject("Key").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.5f;light.transform.rotation=Quaternion.Euler(40,-25,0);
            var go=new GameObject(prop.name);go.AddComponent<MeshFilter>().sharedMesh=prop.mesh;go.AddComponent<MeshRenderer>().sharedMaterials=prop.materials;go.transform.localScale=prop.scale;go.transform.rotation=prop.rotation;
            var c=CameraFor(BoundsOf(go),new Vector3(.3f,.9f,1));string file="material_"+number++;
            Capture(c,file);index.AppendLine(file+"\t"+prop.name+"\t"+string.Join(";",prop.materials.Select(AssetDatabase.GetAssetPath)));
        }
        File.WriteAllText(System.IO.Path.Combine(Out,"material-index.tsv"),index.ToString());
        var traces=new StringBuilder("role\tclip\tphase\tleftY\tleftZ\trightY\trightZ\n");
        foreach(string role in new[]{"MonsterA","Monster2","MonsterC"})
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var go=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/MoMing/FormalLevels/Prefabs/Monster/"+role+".prefab"));go.transform.position=Vector3.zero;go.transform.rotation=Quaternion.identity;
            var animator=go.GetComponentInChildren<Animator>();var bones=animator.GetComponentsInChildren<Transform>();
            var left=bones.FirstOrDefault(t=>t.name.EndsWith("LeftFoot",StringComparison.OrdinalIgnoreCase));var right=bones.FirstOrDefault(t=>t.name.EndsWith("RightFoot",StringComparison.OrdinalIgnoreCase));
            if(left==null||right==null){traces.AppendLine(role+" MISSING FOOT BONES "+string.Join(";",bones.Select(t=>t.name)));continue;}
            foreach(var clip in animator.runtimeAnimatorController.animationClips.Distinct().Where(c=>c.name.Contains("walk")||c.name.Contains("run")))
            {
                AnimationMode.StartAnimationMode();
                for(int i=0;i<120;i++)
                {
                    AnimationMode.BeginSampling();AnimationMode.SampleAnimationClip(animator.gameObject,clip,clip.length*i/120f);AnimationMode.EndSampling();
                    var l=go.transform.InverseTransformPoint(left.position);var r=go.transform.InverseTransformPoint(right.position);
                    traces.AppendLine(role+"\t"+clip.name+"\t"+(i/120f)+"\t"+l.y+"\t"+l.z+"\t"+r.y+"\t"+r.z);
                }
                AnimationMode.StopAnimationMode();
            }
        }
        File.WriteAllText(System.IO.Path.Combine(Out,"gait-traces.tsv"),traces.ToString());EditorApplication.Exit(0);
    }
}
