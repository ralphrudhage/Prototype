using UnityEngine;

namespace Managers
{
    public class CursorManager : MonoBehaviour
    {
        public Texture2D cursorTexNormal;
        public Texture2D cursorTexClicked;

        private void Start()
        {
            SetCursor(cursorTexNormal);
            Cursor.visible = true;
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                SetCursor(cursorTexClicked);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                SetCursor(cursorTexNormal);
            }
        }

        private void SetCursor(Texture2D texture)
        {
            var screenHeight = Screen.height;

            var scaleFactor = screenHeight / 1080f;
            var newWidth = Mathf.RoundToInt(texture.width * scaleFactor);
            var newHeight = Mathf.RoundToInt(texture.height * scaleFactor);

            var scaledTexture = ScaleTexture(texture, newWidth, newHeight);

            Cursor.SetCursor(scaledTexture, Vector2.zero, CursorMode.ForceSoftware);
        }


        private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(source, rt);

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            result.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            return result;
        }
    }
}