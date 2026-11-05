﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace gorsel_programlama_2___ders1___at_yarışı
{
    public partial class Form1 : Form
    {
        // ---- Bir atın bilgileri ----
        class At
        {
            public string Ad;
            public Color Govde, Koyu, Yele, Forma;
            public float X;        // atın pistteki yeri (gövdenin ortası)
            public float Hiz;      // anlık hızı
            public float Adim;     // bacak animasyonu için
            public int Sira;       // bitirdiği sıra (0 = henüz bitmedi)
        }

        Random rastgele = new Random();
        At[] atlar;

        // Pistin ölçüleri
        const int UstBosluk = 96;      // başlığın yüksekliği
        const int SeritYuksekligi = 124;
        const int PistUst = UstBosluk + 34;
        const float AtBuyuklugu = 1.3f;
        int baslangicX = 160;
        int bitisX;

        // Yarışın durumu
        enum Durum { Bekliyor, GeriSayim, Kosuyor, Bitti }
        Durum durum = Durum.Bekliyor;
        int geriSayimTik = 0;
        int yarisTik = 0;
        int bitenSayisi = 0;
        At kazanan = null;
        int tahmin = -1;   // oyuncunun tahmin ettiği at (-1 = tahmin yok)

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;   // titremeyi önler
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

            atlar = new At[]
            {
                new At { Ad = "Kızıl Yıldırım", Govde = Color.FromArgb(150, 82, 42), Koyu = Color.FromArgb(110, 58, 28), Yele = Color.FromArgb(52, 28, 14), Forma = Color.FromArgb(230, 57, 70) },
                new At { Ad = "Beyaz Rüzgar",   Govde = Color.FromArgb(236, 230, 218), Koyu = Color.FromArgb(196, 188, 172), Yele = Color.FromArgb(170, 160, 145), Forma = Color.FromArgb(29, 111, 216) },
                new At { Ad = "Kara Şimşek",    Govde = Color.FromArgb(44, 44, 50), Koyu = Color.FromArgb(24, 24, 28), Yele = Color.FromArgb(10, 10, 12), Forma = Color.FromArgb(245, 183, 0) },
            };
            Sifirla();
        }

        void Sifirla()
        {
            bitisX = ClientSize.Width - 130;
            foreach (At at in atlar)
            {
                at.X = baslangicX - 74 * AtBuyuklugu - 6;   // burnu çizginin hemen gerisinde
                at.Hiz = 0;
                at.Adim = (float)(rastgele.NextDouble() * 6);
                at.Sira = 0;
            }
            bitenSayisi = 0;
            yarisTik = 0;
            kazanan = null;
            durum = Durum.Bekliyor;
            button1.Enabled = true;
            Invalidate();
        }

        // ---------------- Butonlar ----------------
        private void button1_Click(object sender, EventArgs e)
        {
            Sifirla();
            button1.Text = "BAŞLAT";
            durum = Durum.GeriSayim;
            geriSayimTik = 0;
            button1.Enabled = false;
            timer1.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            Sifirla();
        }

        // Alttaki kartlara tıklayınca tahmin seçilir
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (durum == Durum.Kosuyor || durum == Durum.GeriSayim) return;
            for (int i = 0; i < atlar.Length; i++)
            {
                if (KartAlani(i).Contains(e.Location))
                {
                    tahmin = (tahmin == i) ? -1 : i;
                    Invalidate();
                }
            }
        }

        // ---------------- Zamanlayıcı ----------------
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (durum == Durum.GeriSayim)
            {
                geriSayimTik++;
                if (geriSayimTik >= 100) durum = Durum.Kosuyor;   // 3, 2, 1, KOŞ!
                Invalidate();
                return;
            }

            if (durum == Durum.Kosuyor)
            {
                yarisTik++;
                foreach (At at in atlar)
                {
                    if (at.Sira > 0)
                    {
                        // bitirdi: yavaşça dursun
                        at.Hiz = Math.Max(0, at.Hiz - 0.3f);
                    }
                    else
                    {
                        // hız rastgele biraz artar ya da azalır
                        at.Hiz += rastgele.Next(-1, 2) * 0.5f;
                        at.Hiz = Math.Max(2.5f, Math.Min(7.5f, at.Hiz));
                    }
                    at.X += at.Hiz + (float)rastgele.NextDouble() * 1.5f;
                    at.X = Math.Min(at.X, EnSagX());
                    at.Adim += 0.12f + at.Hiz * 0.05f;

                    float burun = at.X + 74 * AtBuyuklugu;
                    if (at.Sira == 0 && burun >= bitisX)
                    {
                        bitenSayisi++;
                        at.Sira = bitenSayisi;
                        if (kazanan == null) kazanan = at;
                    }
                }

                if (bitenSayisi == atlar.Length)
                {
                    durum = Durum.Bitti;
                }
                Invalidate();
                return;
            }

            if (durum == Durum.Bitti)
            {
                // kazanan ekranındayken atlar yavaşlayarak dursun
                bool hepsiDurdu = true;
                foreach (At at in atlar)
                {
                    at.Hiz = Math.Max(0, at.Hiz - 0.3f);
                    at.X = Math.Min(at.X + at.Hiz, EnSagX());
                    if (at.Hiz > 0) { at.Adim += 0.12f + at.Hiz * 0.05f; hepsiDurdu = false; }
                }
                if (hepsiDurdu)
                {
                    timer1.Stop();
                    button1.Enabled = true;
                    button1.Text = "TEKRAR";
                }
                Invalidate();
            }
        }

        // Bitişten sonra at ekranın dışına çıkmasın
        float EnSagX()
        {
            return ClientSize.Width - 20 - 74 * AtBuyuklugu;
        }

        // ---------------- Çizim ----------------
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            BaslikCiz(g);
            PistCiz(g);

            // atlar: üstteki şerit önce çizilsin
            for (int i = 0; i < atlar.Length; i++)
            {
                float zemin = PistUst + i * SeritYuksekligi + SeritYuksekligi - 14;
                AtCiz(g, atlar[i], atlar[i].X, zemin, i + 1);
            }

            AltPanelCiz(g);

            if (durum == Durum.GeriSayim) GeriSayimCiz(g);
            if (durum == Durum.Bitti) KazananCiz(g);
        }

        void BaslikCiz(Graphics g)
        {
            Rectangle r = new Rectangle(0, 0, ClientSize.Width, UstBosluk);
            using (var br = new LinearGradientBrush(r, Color.FromArgb(18, 24, 48), Color.FromArgb(40, 30, 78), 0f))
                g.FillRectangle(br, r);
            using (var cizgi = new Pen(Color.FromArgb(245, 183, 0), 3))
                g.DrawLine(cizgi, 0, UstBosluk - 2, ClientSize.Width, UstBosluk - 2);

            using (var f = new Font("Segoe UI", 26, FontStyle.Bold))
            using (var altF = new Font("Segoe UI", 11))
            {
                g.DrawString("AT YARIŞI", f, Brushes.White, 30, 14);
                using (var b = new SolidBrush(Color.FromArgb(180, 190, 220)))
                    g.DrawString("Bir at seç, tahminini yap, BAŞLAT'a bas!", altF, b, 34, 60);
            }

            // sağ üstte süre
            string sure = (yarisTik * timer1.Interval / 1000.0).ToString("0.0") + " sn";
            using (var f = new Font("Consolas", 22, FontStyle.Bold))
            using (var kutu = new SolidBrush(Color.FromArgb(60, 255, 255, 255)))
            {
                Rectangle k = new Rectangle(ClientSize.Width - 210, 22, 180, 50);
                using (GraphicsPath p = Yuvarlak(k, 12)) g.FillPath(kutu, p);
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using (var b = new SolidBrush(Color.FromArgb(245, 183, 0)))
                    g.DrawString(sure, f, b, k, sf);
            }
        }

        void PistCiz(Graphics g)
        {
            int genislik = ClientSize.Width;
            int pistAlt = PistUst + atlar.Length * SeritYuksekligi;

            // çimen (üst ve alt)
            using (var cim = new LinearGradientBrush(new Rectangle(0, UstBosluk, genislik, PistUst - UstBosluk), Color.FromArgb(46, 125, 50), Color.FromArgb(67, 160, 71), 90f))
                g.FillRectangle(cim, 0, UstBosluk, genislik, PistUst - UstBosluk);
            using (var cim = new SolidBrush(Color.FromArgb(56, 142, 60)))
                g.FillRectangle(cim, 0, pistAlt, genislik, 24);

            // şeritler (kum zemin, sırayla açık/koyu)
            for (int i = 0; i < atlar.Length; i++)
            {
                Rectangle serit = new Rectangle(0, PistUst + i * SeritYuksekligi, genislik, SeritYuksekligi);
                Color ust = i % 2 == 0 ? Color.FromArgb(196, 140, 88) : Color.FromArgb(184, 128, 78);
                Color alt = i % 2 == 0 ? Color.FromArgb(172, 118, 70) : Color.FromArgb(160, 108, 62);
                using (var br = new LinearGradientBrush(serit, ust, alt, 90f)) g.FillRectangle(br, serit);

                // kumda hafif izler
                using (var iz = new Pen(Color.FromArgb(25, 90, 50, 20), 2))
                    for (int x = (i * 37) % 80; x < genislik; x += 80)
                        g.DrawLine(iz, x, serit.Y + 30 + (x % 3) * 25, x + 30, serit.Y + 30 + (x % 3) * 25);

                // şerit numarası
                using (var f = new Font("Segoe UI", 28, FontStyle.Bold))
                using (var b = new SolidBrush(Color.FromArgb(60, 255, 255, 255)))
                    g.DrawString((i + 1).ToString(), f, b, bitisX + 50, serit.Y + 34);
            }

            // beyaz ayırma çizgileri (çit)
            using (var cit = new Pen(Color.White, 4))
            using (var golge = new Pen(Color.FromArgb(70, 0, 0, 0), 2))
            {
                for (int i = 0; i <= atlar.Length; i++)
                {
                    int y = PistUst + i * SeritYuksekligi;
                    g.DrawLine(golge, 0, y + 3, genislik, y + 3);
                    g.DrawLine(cit, 0, y, genislik, y);
                }
            }
            // çit direkleri
            using (var direk = new SolidBrush(Color.White))
                for (int x = 40; x < genislik; x += 120)
                {
                    g.FillRectangle(direk, x, PistUst - 14, 5, 14);
                    g.FillRectangle(direk, x, pistAlt, 5, 14);
                }

            // mesafe işaretleri
            using (var f = new Font("Segoe UI", 9, FontStyle.Bold))
            using (var b = new SolidBrush(Color.FromArgb(220, 255, 255, 255)))
            {
                int toplam = bitisX - baslangicX;
                for (int m = 1; m < 4; m++)
                {
                    int x = baslangicX + toplam * m / 4;
                    g.DrawString((m * 250) + " m", f, b, x - 14, PistUst - 30);
                    using (var p = new Pen(Color.FromArgb(50, 255, 255, 255), 2) { DashStyle = DashStyle.Dash })
                        g.DrawLine(p, x, PistUst, x, pistAlt);
                }
            }

            // başlangıç çizgisi
            using (var p = new Pen(Color.FromArgb(230, 255, 255, 255), 4))
                g.DrawLine(p, baslangicX, PistUst, baslangicX, pistAlt);

            // damalı bitiş çizgisi
            int kare = 12;
            for (int y = PistUst, sat = 0; y < pistAlt; y += kare, sat++)
                for (int k = 0; k < 2; k++)
                {
                    Brush b = (sat + k) % 2 == 0 ? Brushes.White : Brushes.Black;
                    g.FillRectangle(b, bitisX + k * kare, y, kare, Math.Min(kare, pistAlt - y));
                }
            using (var f = new Font("Segoe UI", 10, FontStyle.Bold))
                g.DrawString("BİTİŞ", f, Brushes.White, bitisX - 6, PistUst - 30);
        }

        // Atı jokeyiyle birlikte çizer. (x, zemin) = atın ayaklarının bastığı yer.
        void AtCiz(Graphics g, At at, float x, float zemin, int numara)
        {
            GraphicsState eski = g.Save();
            g.TranslateTransform(x, zemin);
            g.ScaleTransform(AtBuyuklugu, AtBuyuklugu);

            float sw = (float)Math.Sin(at.Adim);
            // dururken bacaklar düz dursun
            float salinim = Math.Min(1f, at.Hiz / 2.5f);
            if (durum == Durum.Bekliyor || durum == Durum.GeriSayim) salinim = 0.15f;

            // gölge
            using (var b = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
                g.FillEllipse(b, -46, -4, 92, 12);

            // arkadaki bacaklar (koyu renk)
            Bacak(g, 26, -40, salinim * (0.9f * sw) + 0.25f, salinim * (0.9f * sw - 0.35f + (sw < 0 ? -0.6f : 0)), at.Koyu);
            float s2 = (float)Math.Sin(at.Adim + 2.2);
            Bacak(g, -24, -42, salinim * (-0.8f * s2) + 0.1f, salinim * (-0.8f * s2) + 0.7f * salinim + 0.1f, at.Koyu);

            // kuyruk
            using (var p = new Pen(at.Yele, 7) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawBezier(p, -34, -50, -46, -56, -56, -46, -62, -34 + sw * 3 * salinim);

            using (var govde = new SolidBrush(at.Govde))
            {
                // gövde
                using (var p = new GraphicsPath())
                {
                    p.AddBezier(-36, -48, -36, -63, -12, -64, 10, -61);
                    p.AddBezier(10, -61, 28, -61, 38, -54, 38, -44);
                    p.AddBezier(38, -44, 38, -33, 22, -30, 0, -30);
                    p.AddBezier(0, -30, -22, -30, -37, -34, -36, -48);
                    p.CloseFigure();
                    g.FillPath(govde, p);
                }
                // boyun
                using (var p = new GraphicsPath())
                {
                    p.AddBezier(18, -56, 26, -68, 32, -80, 40, -90);
                    p.AddLine(40, -90, 52, -86);
                    p.AddBezier(52, -86, 50, -72, 46, -56, 36, -40);
                    p.CloseFigure();
                    g.FillPath(govde, p);
                }
                // baş
                using (var p = new GraphicsPath())
                {
                    p.AddBezier(39, -91, 45, -98, 55, -96, 59, -89);
                    p.AddLine(59, -89, 72, -76);
                    p.AddBezier(72, -76, 74, -71, 69, -67, 64, -69);
                    p.AddLine(64, -69, 50, -76);
                    p.AddBezier(50, -76, 44, -79, 39, -85, 39, -91);
                    p.CloseFigure();
                    g.FillPath(govde, p);
                }
                // kulak
                g.FillPolygon(govde, new PointF[] { new PointF(44, -93), new PointF(46, -103), new PointF(51, -94) });
            }
            // göz ve burun deliği
            g.FillEllipse(Brushes.Black, 51, -89, 4, 4);
            g.FillEllipse(Brushes.Black, 67, -74, 2.6f, 2.6f);

            // yele
            using (var p = new Pen(at.Yele, 6) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawBezier(p, 43, -94, 36, -84, 30, -72, 20, -60);

            // öndeki bacaklar (gövde rengi)
            float s3 = (float)Math.Sin(at.Adim + 0.6);
            Bacak(g, 22, -38, salinim * (-0.9f * s3) + 0.2f, salinim * (-0.9f * s3 - 0.5f), at.Govde);
            float s4 = (float)Math.Sin(at.Adim + 2.8);
            Bacak(g, -20, -40, salinim * (0.8f * s4) + 0.1f, salinim * (0.8f * s4) + 0.7f * salinim + 0.1f, at.Govde);

            // eyer örtüsü ve numara
            using (var b = new SolidBrush(at.Forma))
                g.FillPolygon(b, new PointF[] { new PointF(-14, -60), new PointF(8, -61), new PointF(9, -42), new PointF(-13, -41) });
            using (var f = new Font("Segoe UI", 9, FontStyle.Bold))
            {
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(numara.ToString(), f, Brushes.White, new RectangleF(-14, -60, 23, 19), sf);
            }

            // jokey
            Cizgi(g, 0, -64, 10, -56, 6, Color.FromArgb(244, 244, 244));     // pantolon
            Cizgi(g, 10, -56, 4, -47, 5, Color.FromArgb(51, 51, 51));         // çizme
            Cizgi(g, -2, -64, 10, -80, 11, at.Forma);                          // gövde
            Cizgi(g, 8, -77, 26, -70, 4, at.Forma);                            // kol
            using (var b = new SolidBrush(Color.FromArgb(241, 194, 125)))
                g.FillEllipse(b, 8, -93, 12, 12);                              // yüz
            using (var b = new SolidBrush(at.Forma))
                g.FillPie(b, 7.5f, -94.5f, 13, 13, 180, 180);                  // kask
            Cizgi(g, 26, -70, 40, -82, 1.2f, Color.FromArgb(90, 58, 26));     // dizgin

            g.Restore(eski);
        }

        // Bir bacak: kalçadan dize, dizden toynağa iki parça
        void Bacak(Graphics g, float kalcaX, float kalcaY, float aci1, float aci2, Color renk)
        {
            const float ust = 20, alt = 19;
            float dizX = kalcaX + (float)Math.Sin(aci1) * ust;
            float dizY = kalcaY + (float)Math.Cos(aci1) * ust;
            float ayakX = dizX + (float)Math.Sin(aci2) * alt;
            float ayakY = dizY + (float)Math.Cos(aci2) * alt;
            Cizgi(g, kalcaX, kalcaY, dizX, dizY, 8, renk);
            Cizgi(g, dizX, dizY, ayakX, ayakY, 5, renk);
            using (var b = new SolidBrush(Color.FromArgb(34, 34, 34)))
                g.FillEllipse(b, ayakX - 3.5f, ayakY - 3.5f, 7, 7);
        }

        void Cizgi(Graphics g, float x1, float y1, float x2, float y2, float kalinlik, Color renk)
        {
            using (var p = new Pen(renk, kalinlik) { StartCap = LineCap.Round, EndCap = LineCap.Round })
                g.DrawLine(p, x1, y1, x2, y2);
        }

        // ---------------- Alt panel ----------------
        int PanelUst { get { return PistUst + atlar.Length * SeritYuksekligi + 24; } }

        Rectangle KartAlani(int i)
        {
            int kartGenislik = (ClientSize.Width - 300) / atlar.Length;
            return new Rectangle(24 + i * (kartGenislik + 12), PanelUst + 16, kartGenislik, ClientSize.Height - PanelUst - 32);
        }

        void AltPanelCiz(Graphics g)
        {
            Rectangle panel = new Rectangle(0, PanelUst, ClientSize.Width, ClientSize.Height - PanelUst);
            using (var br = new LinearGradientBrush(panel, Color.FromArgb(24, 28, 52), Color.FromArgb(14, 16, 30), 90f))
                g.FillRectangle(br, panel);

            float toplam = bitisX - baslangicX;
            for (int i = 0; i < atlar.Length; i++)
            {
                At at = atlar[i];
                Rectangle k = KartAlani(i);
                bool secili = (tahmin == i);

                using (GraphicsPath p = Yuvarlak(k, 14))
                {
                    using (var b = new SolidBrush(secili ? Color.FromArgb(60, at.Forma) : Color.FromArgb(34, 40, 70)))
                        g.FillPath(b, p);
                    using (var kenar = new Pen(secili ? at.Forma : Color.FromArgb(60, 70, 110), secili ? 3 : 1))
                        g.DrawPath(kenar, p);
                }

                // renkli numara rozeti
                Rectangle rozet = new Rectangle(k.X + 14, k.Y + 14, 36, 36);
                using (var b = new SolidBrush(at.Forma)) g.FillEllipse(b, rozet);
                StringFormat orta = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using (var f = new Font("Segoe UI", 13, FontStyle.Bold))
                    g.DrawString((i + 1).ToString(), f, Brushes.White, rozet, orta);

                using (var f = new Font("Segoe UI", 13, FontStyle.Bold))
                    g.DrawString(at.Ad, f, Brushes.White, k.X + 58, k.Y + 12);
                using (var f = new Font("Segoe UI", 9))
                using (var b = new SolidBrush(Color.FromArgb(160, 170, 200)))
                {
                    string alt = secili ? "★ Senin tahminin" : "Tahmin için tıkla";
                    if (at.Sira > 0) alt = at.Sira + ". oldu";
                    g.DrawString(alt, f, b, k.X + 60, k.Y + 36);
                }

                // ilerleme çubuğu
                float oran = Math.Max(0, Math.Min(1, (at.X + 74 * AtBuyuklugu - baslangicX) / (toplam + 20)));
                if (at.Sira > 0) oran = 1;
                Rectangle cubuk = new Rectangle(k.X + 14, k.Bottom - 34, k.Width - 28, 18);
                using (GraphicsPath p = Yuvarlak(cubuk, 9))
                using (var b = new SolidBrush(Color.FromArgb(14, 16, 30)))
                    g.FillPath(b, p);
                int dolu = (int)(cubuk.Width * oran);
                if (dolu > 18)
                {
                    Rectangle d = new Rectangle(cubuk.X, cubuk.Y, dolu, cubuk.Height);
                    using (GraphicsPath p = Yuvarlak(d, 9))
                    using (var b = new LinearGradientBrush(d, ControlPaint.Light(at.Forma), at.Forma, 0f))
                        g.FillPath(b, p);
                }
                using (var f = new Font("Segoe UI", 8, FontStyle.Bold))
                    g.DrawString("%" + (int)(oran * 100), f, Brushes.White, cubuk.Right - 36, cubuk.Y + 2);
            }
        }

        void GeriSayimCiz(Graphics g)
        {
            string yazi;
            if (geriSayimTik < 25) yazi = "3";
            else if (geriSayimTik < 50) yazi = "2";
            else if (geriSayimTik < 75) yazi = "1";
            else yazi = "KOŞ!";

            // her saniyenin başında büyük, sonra küçülür
            float t = (geriSayimTik % 25) / 25f;
            float boyut = 90 - 30 * t;
            int alfa = (int)(255 * (1 - t * 0.6f));

            Rectangle alan = new Rectangle(0, PistUst, ClientSize.Width, atlar.Length * SeritYuksekligi);
            using (var b = new SolidBrush(Color.FromArgb(70, 0, 0, 0))) g.FillRectangle(b, alan);
            StringFormat orta = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using (var f = new Font("Segoe UI", boyut, FontStyle.Bold))
            {
                using (var golge = new SolidBrush(Color.FromArgb(alfa / 2, 0, 0, 0)))
                    g.DrawString(yazi, f, golge, new Rectangle(alan.X + 4, alan.Y + 4, alan.Width, alan.Height), orta);
                using (var b = new SolidBrush(Color.FromArgb(alfa, 245, 183, 0)))
                    g.DrawString(yazi, f, b, alan, orta);
            }
        }

        void KazananCiz(Graphics g)
        {
            if (kazanan == null) return;
            Rectangle kutu = new Rectangle(ClientSize.Width / 2 - 260, PistUst + 60, 520, 230);

            using (var b = new SolidBrush(Color.FromArgb(110, 0, 0, 0)))
                g.FillRectangle(b, 0, PistUst, ClientSize.Width, atlar.Length * SeritYuksekligi);

            using (GraphicsPath p = Yuvarlak(kutu, 22))
            {
                using (var b = new LinearGradientBrush(kutu, Color.FromArgb(40, 30, 78), Color.FromArgb(18, 24, 48), 90f))
                    g.FillPath(b, p);
                using (var kenar = new Pen(Color.FromArgb(245, 183, 0), 4))
                    g.DrawPath(kenar, p);
            }

            StringFormat orta = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using (var f = new Font("Segoe UI", 14, FontStyle.Bold))
            using (var b = new SolidBrush(Color.FromArgb(245, 183, 0)))
                g.DrawString("KAZANAN", f, b, new Rectangle(kutu.X, kutu.Y + 14, kutu.Width, 34), orta);

            // kupa
            KupaCiz(g, kutu.X + kutu.Width / 2, kutu.Y + 90);

            int no = Array.IndexOf(atlar, kazanan) + 1;
            using (var f = new Font("Segoe UI", 24, FontStyle.Bold))
            using (var b = new SolidBrush(kazanan.Forma == Color.FromArgb(29, 111, 216) ? Color.FromArgb(120, 175, 255) : kazanan.Forma))
                g.DrawString(no + " - " + kazanan.Ad, f, b, new Rectangle(kutu.X, kutu.Y + 120, kutu.Width, 46), orta);

            string alt = "Süre: " + (yarisTik * timer1.Interval / 1000.0).ToString("0.0") + " sn";
            if (tahmin >= 0)
                alt += (atlar[tahmin] == kazanan) ? "   •   Tahminin tuttu!" : "   •   Tahminin tutmadı";
            using (var f = new Font("Segoe UI", 12))
            using (var b = new SolidBrush(Color.FromArgb(200, 210, 235)))
                g.DrawString(alt, f, b, new Rectangle(kutu.X, kutu.Y + 172, kutu.Width, 36), orta);
        }

        void KupaCiz(Graphics g, int x, int y)
        {
            Color altin = Color.FromArgb(245, 183, 0);
            using (var b = new SolidBrush(altin))
            using (var p = new Pen(altin, 4))
            {
                g.FillPie(b, x - 20, y - 46, 40, 44, 0, 180);       // kase
                g.FillRectangle(b, x - 20, y - 26, 40, 4);
                g.DrawArc(p, x - 30, y - 22 - 14, 16, 18, 90, 180);  // sol kulp
                g.DrawArc(p, x + 14, y - 22 - 14, 16, 18, 270, 180); // sağ kulp
                g.FillRectangle(b, x - 3, y - 4, 6, 10);             // sap
                g.FillRectangle(b, x - 13, y + 6, 26, 6);            // taban
            }
        }

        // Köşeleri yuvarlatılmış dikdörtgen
        GraphicsPath Yuvarlak(Rectangle r, int yaricap)
        {
            int d = yaricap * 2;
            GraphicsPath p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}
