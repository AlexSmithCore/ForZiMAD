using UnityEngine;

namespace Player{
    public class PlayerManager : MonoBehaviour
    {
        #region  Singleton
        public static PlayerManager instance;

        void Awake(){
            instance = this;
        }

        #endregion

        public int gold = 500;

        public void Payment(int cost){
            gold -= cost;
        }
    }
}
