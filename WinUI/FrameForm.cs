using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaptureStacker.WinUI
{
    public partial class FrameForm : Form
    {
        private static readonly Color primaryTransparencyKey = Color.White;

        // 枠のサイズ
        public int frameBorderSize = Convert.ToInt32(ConfigurationManager.AppSettings["FrameBorderSize"]);

        // 枠の色
        private Color frameBorderColor = Color.Yellow;

        // 枠線描画フラグ
        public bool IsView = false;

        // マウスがフォーム上にあるかどうか
        public bool IsMoseHover = false;

        // サイズ変更、移動の状態
        public transform IsTransform;

        public enum transform
        {
            INVALID,
            NWSE,
            NESW,
            ALL
        }

        public Point mouseDownPoint = new Point();


        #region 枠内容補正値
        // 枠内の画像を取得するために補正した値
        public int CorrectLeft
        {
            get { return this.Left + this.frameBorderSize; }
        }

        public int CorrectTop
        {
            get { return this.Top + this.frameBorderSize; }
        }

        public Point CorrectLocation
        {
            get { return new Point(this.CorrectLeft, this.CorrectTop); }
        }

        public int CorrectWidth
        {
            get { return this.Width - this.frameBorderSize * 2; }
        }

        public int CorrectHeight
        {
            get { return this.Height - this.frameBorderSize * 2; }
        }
        #endregion


        #region コンストラクタ
        public FrameForm()
        {
            InitializeComponent();
        }

        private FrameForm(Color color)
        {
            this.frameBorderColor = color;
        }
        #endregion


        /// <summary>
        /// InitializeComponent()の処理順序ではSizeにデフォルトの最小値が入るためLoadイベントで設定する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrameForm_Load(object sender, EventArgs e)
        {
            this.Size = new Size(1, 1);
            this.TransparencyKey = primaryTransparencyKey;
        }


        /// <summary>
        /// 枠だけ残して透明にする
        /// </summary>
        /// <param name="g"></param>
        private void draw(Graphics g)
        {
            var rct = this.ClientRectangle;

            // 一度枠の色で塗りつぶし、枠サイズ * -1　の領域を透明にする
            g.FillRectangle(new SolidBrush(frameBorderColor), rct);
            rct.Inflate(frameBorderSize * -1, frameBorderSize * -1);
            g.FillRectangle(new SolidBrush(TransparencyKey), rct);
        }


        /// <summary>
        /// 渡される座標によって自身の大きさを変更する
        /// </summary>
        /// <param name="x">マウスカーソルのx座標</param>
        /// <param name="y">マウスカーソルのy座標</param>
        public void CreateFrame(int x, int y)
        {
            // 座標がマイナス方向にも対応する
            Point start = new Point();
            start.X = Math.Min (mouseDownPoint.X, x);
            start.Y = Math.Min(mouseDownPoint.Y, y);

            Point end = new Point();
            end.X = Math.Max(mouseDownPoint.X, x);
            end.Y = Math.Max(mouseDownPoint.Y, y);

            this.Width = Math.Abs(start.X - end.X);
            this.Height = Math.Abs(start.Y - end.Y);

            int corectLocationX = 0;
            int corectLocationY = 0;

            // 選択座標を枠線のサイズ分補正する
            corectLocationX = mouseDownPoint.X < x ? start.X - this.frameBorderSize : start.X + this.frameBorderSize;
            corectLocationY = mouseDownPoint.Y < y ? start.Y - this.frameBorderSize : start.Y + this.frameBorderSize;

            this.Location = new Point(corectLocationX, corectLocationY);

            this.Show();
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            this.draw(e.Graphics);
            base.OnPaint(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.Refresh();
        }


        /// <summary>
        /// マウスカーソルの形状で変更状態を設定する
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (this.IsView)
            {
                return;
            }

            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                //位置を記憶する
                this.mouseDownPoint = new Point(e.X, e.Y);


                if (Cursor == Cursors.SizeNESW)
                {
                    this.IsTransform = transform.NESW;
                }
                else if (Cursor == Cursors.SizeNWSE)
                {
                    this.IsTransform = transform.NWSE;
                }
                else
                {
                    this.IsTransform = transform.ALL;
                }
            }
        }


        /// <summary>
        /// マウスの座標によってカーソルを変更する
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            this.IsMoseHover = true;
            int corectSize = this.frameBorderSize * 2;

            // カーソルが左下の場合
            if (e.X <= corectSize && e.Y >= this.Height - corectSize)
            {
                Cursor = Cursors.SizeNESW;

            }
            // カーソルが右下の場合
            else if (e.X >= Width - corectSize && e.Y >= Height - corectSize)
            {
                Cursor = Cursors.SizeNWSE;
            }
            else
            {
                Cursor = Cursors.SizeAll;
            }

            this.frameTransform(e);
        }


        /// <summary>
        /// マウス座標がフレーム上にあるか判定する
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool IsMouseHover(int x, int y)
        {
            bool res = false;

            if(x <= this.Location.X + this.frameBorderSize && x >= this.Location.X ||
               x <= this.Location.X + this.Width && x >= this.Location.X + this.Width - this.frameBorderSize ||
               y <= this.Location.Y + this.frameBorderSize && y >= this.Location.Y ||
               y <= this.Location.Y + this.Height && y >= this.Location.Y + this.Height - this.frameBorderSize)
            {
                res = true;
            }

            return res;
        }


        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            this.IsTransform = default;
        }



        private void frameTransform(MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                // ドラッグにより移動した距離を計算
                int distanceX = e.X - this.mouseDownPoint.X;
                int distanceY = e.Y - this.mouseDownPoint.Y;


                switch (this.IsTransform)
                {
                    case transform.NESW:

                        int w = this.Width;
                        this.Width -= distanceX;
                        this.Left += w - this.Width;
                        this.Height = this.mouseDownPoint.Y + distanceY;

                        break;

                    case transform.NWSE:

                        this.Width = this.mouseDownPoint.X + distanceX;
                        this.Height = this.mouseDownPoint.Y + distanceY;

                        break;

                    case transform.ALL:

                        this.Left += e.X - this.mouseDownPoint.X;
                        this.Top += e.Y - this.mouseDownPoint.Y;

                        break;
                }
            }
        }

    }
}
