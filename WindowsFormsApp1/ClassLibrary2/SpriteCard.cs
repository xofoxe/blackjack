using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ClassLibrary1
{
    public class SpriteCard : PictureBox
    {

        public string CardCode { get; private set; }
        private Image frontImage;
        private Image backImage;
        private bool isFaceUp;

        public bool IsFaceUp => isFaceUp;

        public SpriteCard(string cardCode, bool faceUp = false)
        {
            CardCode = cardCode;
            frontImage = LoadCardImage(CardCode);
            backImage = Image.FromFile($"../../Assets/Cards/cardBack_red2.png");
            isFaceUp = faceUp;

            this.Image = isFaceUp ? frontImage : backImage;
            this.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private Image LoadCardImage(string code)
        {
            try
            {
                return Image.FromFile($"../../Assets/Cards/{CardCode}.png");
            }
            catch
            {
                return new Bitmap(80, 120);
            }
        }

        private void UpdateImage()
        {
            Image = isFaceUp ? frontImage : backImage;
        }

        public void Flip()
        {
            isFaceUp = !isFaceUp;
            UpdateImage();
        }

        public void SetFaceUp(bool faceUp)
        {
            isFaceUp = faceUp;
            UpdateImage();
        }

        public void SetCard(string newCode)
        {
            CardCode = newCode;
            frontImage = LoadCardImage(newCode);
            UpdateImage();
        }

    }
}

