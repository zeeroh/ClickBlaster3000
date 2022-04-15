using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Gma.System.MouseKeyHook;

namespace ClickBlaster3002
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int GWL_EXSTYLE = (-20);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);


        private List<Sprite> SpriteList = new List<Sprite>();
        public int FrameWidth = 256;
        public int FrameHeight = 256;
        private bool canSpritesDie = true;

        public class Sprite
        {
            public Rectangle frame = new Rectangle();
            public int NumberOfColumns = 8;
            public int NumberOfFrames = 64;
            public int TargetFramesPerSecond = 60;
            public int currentFrame;
            /*public TimeSpan TimePerFrame = TimeSpan.FromSeconds(1 / TargetFramesPerSecond);*/
            // TODO: need to figure out how to make the bottom line dynamic like the above line again
            public TimeSpan TimePerFrame = TimeSpan.FromSeconds(1 / 60);
            public TimeSpan timeTillNextFrame;

            public Sprite(Rectangle rect)
            {
                this.frame = rect;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            CompositionTarget.Rendering += this.OnUpdate;
            DoCompact();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            this.Topmost = true;
            this.Top = 0;
            this.Left = 0;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.Topmost = true;
            this.Activate();
        }

        public void CreateNewSprite(int xPos, int yPos)
        {
            // TODO: figure out if it's faster to just create a dozen sprites ready to be used
            // over and over or if it's good enough just to create and destroy them as needed?
            Sprite mySprite = Initialize_Sprite();
            canvas.Children.Add(mySprite.frame); // need to draw the sprite to the canvas to see it!
            Canvas.SetTop(mySprite.frame, yPos - (FrameHeight/2)); // this is the y-placement of the sprite on the screen
            Canvas.SetLeft(mySprite.frame, xPos - (FrameWidth/2)); // this is the x-placement of the sprite on the screen
            SpriteList.Add(mySprite);
        }

        private Sprite Initialize_Sprite()
        {
            Rectangle rectangle = new Rectangle();
            rectangle.Width = FrameWidth;
            rectangle.Height = FrameHeight;

            Random rnd = new Random();
            int spriteVariation = rnd.Next(1, 5); // select random int between 1 and 4
            Uri spriteFile = new Uri($"pack://application:,,,/images/{spriteVariation}.png", UriKind.Absolute);

            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(spriteFile);
            imageBrush.Stretch = Stretch.None;
            imageBrush.AlignmentX = AlignmentX.Left;
            imageBrush.AlignmentY = AlignmentY.Top;
            imageBrush.Viewport = new Rect(0, 0, FrameWidth, FrameHeight);
            imageBrush.Transform = new TranslateTransform(0d,0d);

            rectangle.Fill = imageBrush;
            Sprite mySprite = new Sprite(rectangle);
            return mySprite;
        }

        private void OnUpdate(object sender, object e)
        {
            foreach (var sprite in SpriteList.ToList())
            {
                sprite.timeTillNextFrame += TimeSpan.FromSeconds(1 / 60f);
                if (sprite.timeTillNextFrame >= sprite.TimePerFrame)
                {
                    sprite.currentFrame = (sprite.currentFrame + 1 + sprite.NumberOfFrames) % sprite.NumberOfFrames;
                    var column = sprite.currentFrame % sprite.NumberOfColumns;
                    var row = sprite.currentFrame / sprite.NumberOfColumns;

                    sprite.frame.Fill.Transform = new TranslateTransform(-column * this.FrameWidth, -row * this.FrameHeight);
                    sprite.timeTillNextFrame = TimeSpan.Zero;
                    if (sprite.currentFrame == 0 && canSpritesDie)
                    {
                        canvas.Children.Remove(sprite.frame);
                        SpriteList.Remove(sprite);
                    }
                }
            }
        }

        public void DoCompact()
        {
            void DoSomething(System.Windows.Forms.MouseEventArgs e)
            {
                CreateNewSprite(e.X, e.Y);
            }

            Hook.GlobalEvents().MouseDown += async (sender, e) =>
            {
                DoSomething(e);
                Console.WriteLine($"Mouse {e.Button} Down");
            };

/*            Hook.GlobalEvents().OnCombination(new Dictionary<Combination, Action>
            {
                {Combination.FromString("Control+O"), () => { canSpritesDie = !canSpritesDie; }}
            });*/
        }

        // make opaque window click-throughable:
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Get this window's handle
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;

            // Change the extended window style to include WS_EX_TRANSPARENT
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }
    }
}
