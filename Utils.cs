using Newtonsoft.Json;
using Satchel;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Hat
{
    internal static class Utils {


        internal static System.Random rand = new System.Random();
        internal static Vector3 ZeroVector = new Vector3(0,0,0);
        internal static void SerialiseSetting(string path,Setting setting){
            var Json = JsonConvert.SerializeObject(setting, Formatting.Indented);
            File.WriteAllText(path,Json);
        }

        internal static Setting DeSerialiseSetting(string path){
            var Json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Setting>(Json, new JsonSerializerSettings() { ObjectCreationHandling = ObjectCreationHandling.Replace });
        }
        internal static void SetScale(this GameObject gameObject,float scale){
            var localScale = gameObject.transform.localScale;
            localScale.x = Math.Abs(scale);
            localScale.y = Math.Abs(scale);
            gameObject.transform.localScale = localScale;
        }

        internal static void ExtractHatPng(string path){
            
            Assembly asm = Assembly.GetExecutingAssembly();
            foreach (string res in asm.GetManifestResourceNames())
            {   
                if(!res.EndsWith(Constants.DEFAULT_HAT)) {
                    continue;
                } 
                using (Stream s = asm.GetManifestResourceStream(res))
                {
                        if (s == null) continue;
                        var buffer = new byte[s.Length];
                        s.Read(buffer, 0, buffer.Length);
                        File.WriteAllBytes(path,buffer);
                        s.Dispose();
                }
            }
        }

        
        internal static Texture2D LoadTexture(string currentDirectory,string name){
            return TextureUtils.LoadTextureFromFile(Path.Combine(currentDirectory,name));
        }

        internal static Vector3 GetParentColliderCenter(GameObject Parent){
            var collider = Parent.GetComponent<Collider2D>();
            if(collider != null){
                return collider.bounds.center;
            } 
            return ZeroVector;
        }
    }
}
