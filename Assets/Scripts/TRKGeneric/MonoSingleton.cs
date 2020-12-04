using UnityEngine;

//singleton template class, implement like: public class UIManager : MonoSingleton<UIManager>
//if script uses Awake it will override this Awake

namespace TRKGeneric
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject(typeof(T).ToString());
                    go.AddComponent<T>();
                }
                return _instance;
            }
        }
        private void Awake()
        {
            _instance = this as T;
            Init();
        }
        public virtual void Init()
        {
            //optional to override
        }
    }
}
