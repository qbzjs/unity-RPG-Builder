using UnityEngine;

namespace BLINK.RPGBuilder.Managers
{
    public class CursorManager : MonoBehaviour
    {
        public enum cursorType
        {
            merchant,
            questGiver,
            interactiveObject,
            craftingStation,
            enemy
        }

        public Texture2D defaultCursor, merchant, questGiver, interactiveObject, craftingStation, enemyCursor;


        public void SetCursor(cursorType type)
        {
            switch (type)
            {
                case cursorType.merchant:
                    Cursor.SetCursor(merchant, Vector2.zero, CursorMode.Auto);
                    break;
                case cursorType.questGiver:
                    Cursor.SetCursor(questGiver, Vector2.zero, CursorMode.Auto);
                    break;
                case cursorType.interactiveObject:
                    Cursor.SetCursor(interactiveObject, Vector2.zero, CursorMode.Auto);
                    break;
                case cursorType.craftingStation:
                    Cursor.SetCursor(craftingStation, Vector2.zero, CursorMode.Auto);
                    break;
                case cursorType.enemy:
                    Cursor.SetCursor(enemyCursor, Vector2.zero, CursorMode.Auto);
                    break;
            }
        }

        public void ResetCursor()
        {
            Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
        }

        public static CursorManager Instance { get; private set; }

        private void Start()
        {
            if (Instance != null) return;
            Instance = this;
        }
    }
}