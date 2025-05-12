using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClassLibrary1
{
    public class CardAnimator
    {
        private readonly Control _parent;

        public event Action OnAnimEnd;
        public CardAnimator(Control parent)
        {
            _parent = parent;
        }

        public void AnimateCard(Point start, Point end, string spriteCode, bool faceUp)
        {
            var animatedCard = new SpriteCard("cardBack_blue5", false)
            {
                Location = start,
                Size = new Size(80, 120),
            };
            animatedCard.BringToFront();
            _parent.Controls.Add(animatedCard);

            int steps = 40;
            int currentStep = 0;

            Timer timer = new Timer();
            timer.Interval = 10;
            timer.Tick += (s, e) =>
            {
                double t = (double)currentStep / steps;
                double ease = t * (2 - t);

                int x = (int)(start.X + (end.X - start.X) * ease);
                int y = (int)(start.Y + (end.Y - start.Y) * ease);

                animatedCard.Location = new Point(x, y);
                currentStep++;

                if (currentStep > steps)
                {
                    timer.Stop();
                    timer.Dispose();

                    _parent.Controls.Remove(animatedCard);
                    animatedCard.Dispose();

                    var realCard = new SpriteCard(spriteCode, faceUp)
                    {
                        Size = new Size(80, 120),
                        Location = end
                    };

                    _parent.Controls.Add(realCard);
                    realCard.BringToFront();
                }
            };

            timer.Start();
        }


    }
}