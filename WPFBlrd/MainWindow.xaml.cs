using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WPFBlrd
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class billard : Window
    {
        startdlg m_dlg;

        string m_name1, m_name2;

        Rectangle m_table = new Rectangle();
        Rectangle m_frame = new Rectangle();
        Point m_pt, m_offset;
        
        ball[] m_balls;
        cue m_cue;

        DateTime m_startTime = new DateTime();

        int m_hitcounter_01 = 0, m_hitcounter_02 = 0, m_hitcounter_12 = 0;
        int m_player = 0;
        int m_points_0 = 0, m_points_1 = 0;
        int m_shotcounter_0 = 0, m_shotcounter_1 = 0;
        bool m_started = false;
        bool m_takeaim = false;

        System.Media.SoundPlayer player = new System.Media.SoundPlayer("Sounds/click.wav");

        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern uint GetTickCount();

        uint m_ticks = GetTickCount();

        public billard()
        {
            m_dlg = new startdlg();

            if ((bool)m_dlg.ShowDialog())
            {
                m_name1 = m_dlg.m_name1;
                m_name2 = m_dlg.m_name2;
            }
            else
            {
                Close();
            }

            InitializeComponent();

            spieler1.FontFamily = new FontFamily("Courier New");
            spieler1.FontSize = 20F;
            spieler1.FontWeight = FontWeights.Bold;
            spieler1.Foreground = Brushes.Green;
            spieler2.FontFamily = new FontFamily("Courier New");
            spieler2.FontSize = 20F;
            spieler2.FontWeight = FontWeights.Regular;
            spieler2.Foreground = Brushes.Gray;
            zeit.FontFamily = new FontFamily("Courier New");
            zeit.FontSize = 20F;
            zeit.FontWeight = FontWeights.Bold;
            zeit.Foreground = Brushes.DarkCyan;

            Canvas.SetLeft(m_frame, 135);
            Canvas.SetTop(m_frame, 195);

            LinearGradientBrush brush = new LinearGradientBrush();
            brush.GradientStops.Add(new GradientStop(Colors.Brown, 0.0));
            brush.GradientStops.Add(new GradientStop(Colors.Chocolate, 1.0));

            m_frame.Fill = brush;
            
            m_frame.Width = 630;          // SetBottom, SetRight reicht nicht ==> kein Rechteck zu sehen
            m_frame.Height = 330;
            m_frame.RadiusX = 15;
            m_frame.RadiusY = 15;
            m_canvas.Children.Add(m_frame);

            Canvas.SetLeft(m_table, 150);
            Canvas.SetTop(m_table, 210);
            m_table.Fill = System.Windows.Media.Brushes.Green;
            m_table.Width = 600;          
            m_table.Height = 300;
            m_canvas.Children.Add(m_table);

            m_balls = new ball[3];
            m_balls[0] = new ball(350, 800, m_dlg.m_radius, 0.0005, System.Drawing.Color.White);
            m_balls[1] = new ball(280, 220, m_dlg.m_radius, 0.0005, System.Drawing.Color.Yellow);
            m_balls[2] = new ball(420, 220, m_dlg.m_radius, 0.0005, System.Drawing.Color.Red);

            foreach (ball ball in m_balls)
            {
                m_canvas.Children.Add(ball.m_theball);    // Damit wird der Ball gerendert
            }

            m_cue = new cue();
            m_canvas.Children.Add(m_cue.m_line);
            m_canvas.Children.Add(m_cue.m_laser);
            m_canvas.Children.Add(m_cue.m_knob);

            radius.Value = m_dlg.m_radius;
            impulse.Value = 1000;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            dispatcherTimer.Start();

            resetScoreEtc();
        }

        public void resetScoreEtc()
        {
            m_hitcounter_01 = m_hitcounter_02 = m_hitcounter_12 = 0;
            m_player = 0;
            m_points_0 = m_points_1 = 0;
            m_shotcounter_0 = m_shotcounter_1 = 0;
            m_started = false;
            m_takeaim = false;
            
            m_startTime = DateTime.Now;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            uint ticks = GetTickCount();

            foreach (ball ball in m_balls)
            {
                if (!ball.m_isDragged)
                {
                    ball.move((double)(ticks - m_ticks));
                    ball.collision(m_table);
                    ball.friction((double)(ticks - m_ticks));
                }
            }

            if (!m_balls[0].m_isDragged && !m_balls[1].m_isDragged && !m_balls[2].m_isDragged)
            {
                if (m_balls[0].collision(m_balls[1], ticks - m_ticks))
                {
                    player.Play();
                    m_hitcounter_01++;
                }
                if (m_balls[0].collision(m_balls[2], ticks - m_ticks))
                {
                    player.Play();
                    m_hitcounter_02++;
                }
                if (m_balls[1].collision(m_balls[2], ticks - m_ticks))
                {
                    player.Play();
                    m_hitcounter_12++;
                }

                countPoints();

                if (m_player == 0)
                {
                    spieler1.Foreground = Brushes.Green;
                    spieler1.FontWeight = FontWeights.Bold;
                    spieler2.Foreground = Brushes.Gray;
                    spieler2.FontWeight = FontWeights.Regular;
                }
                else
                {
                    spieler1.Foreground = Brushes.Gray;
                    spieler1.FontWeight = FontWeights.Regular;
                    spieler2.Foreground = Brushes.Green;
                    spieler2.FontWeight = FontWeights.Bold;
                }

            }

            spieler1.Content = m_name1 + ": " + m_points_0 + "/" + m_shotcounter_0;
            spieler2.Content = m_name2 + ": " + m_points_1 + "/" + m_shotcounter_1;
            
            zeit.Content = "Spielzeit: " + DateTime.Now.Subtract(m_startTime).ToString(@"m\ \M\i\n\ s\ \S\e\k");

            foreach (ball ball in m_balls)
            {
                if (ball.m_isDragged)
                {
                    ball.drag(m_pt.X + m_offset.X, m_pt.Y + m_offset.Y, m_cue);
                }
            }

            if (m_cue.m_isDragged)
            {
                m_cue.drag(m_pt.X, m_pt.Y, m_balls[m_player]);
            }

            m_cue.laser(m_takeaim);

            m_ticks = ticks;
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                m_pt = e.GetPosition(null);

                bool dragging = false;

                foreach (ball ball in m_balls) {
                    if (ball.m_isDragged)
                    {
                        dragging = true;
                        break;
                    }
                }

                if (m_cue.m_isDragged)
                {
                    dragging = true;
                }

                if (!dragging)
                {
                    foreach (ball ball in m_balls)
                    {
                        if (ball.PtInBall(m_pt.Y, m_pt.X))
                        {
                            ball.m_isDragged = true;
                            m_offset = new Point(ball.m_y - m_pt.X, ball.m_x - m_pt.Y);
                        }
                    }

                    if (m_cue.PtInKnob(m_pt.Y, m_pt.X))
                    {
                        m_cue.m_isDragged = true;
                    }
                }
            }
            else if (e.RightButton == MouseButtonState.Pressed) 
            {
                m_started = true;
                m_hitcounter_01 = m_hitcounter_02 = m_hitcounter_12 = 0;
                
                m_cue.shoot(m_balls[m_player]);

                if (m_player == 0)
                {
                    m_shotcounter_0++;
                }
                else
                {
                    m_shotcounter_1++;
                }
            }
        }

        public void countPoints()
        {
            // Wenn alle Kugeln nach einem Stoß (m_started == true) wieder ruhen
            if (m_started &&
                 m_balls[0].m_vx == 0 && m_balls[0].m_vy == 0 &&
                 m_balls[1].m_vx == 0 && m_balls[1].m_vy == 0 &&
                 m_balls[2].m_vx == 0 && m_balls[2].m_vy == 0)
            {
                if (m_player == 0)
                {
                    if (m_hitcounter_01 == 0 || m_hitcounter_02 == 0)
                    {
                        m_player = 1;
                        if (!(m_hitcounter_01 > 0 || m_hitcounter_02 > 0))
                        {
                            m_points_0--;
                        }
                    }
                    else
                    {
                        m_points_0++;
                    }
                    m_hitcounter_01 = m_hitcounter_02 = 0;
                }
                else
                {
                    if (m_hitcounter_01 == 0 || m_hitcounter_12 == 0)
                    {
                        m_player = 0;
                        if (!(m_hitcounter_01 > 0 || m_hitcounter_12 > 0))
                        {
                            m_points_1--;
                        }
                    }
                    else
                    {
                        m_points_1++;
                    }
                    m_hitcounter_01 = m_hitcounter_12 = 0;
                }
                m_started = false;
            }
        }

        private void Window_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!(m_balls[0].distance(m_balls[1]) <= 2 * m_balls[0].m_radius ||
                 m_balls[0].distance(m_balls[2]) <= 2 * m_balls[0].m_radius ||
                 m_balls[1].distance(m_balls[2]) <= 2 * m_balls[0].m_radius))
            {
                foreach (ball ball in m_balls)
                {
                    ball.m_isDragged = false;
                }
            }
            m_cue.m_isDragged = false;
        }

        private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            foreach (ball ball in m_balls)
            {
                if (ball.m_isDragged)
                {
                    m_pt = e.GetPosition(null);
                }
            }

            if (m_cue.m_isDragged)
            {
                m_pt = e.GetPosition(null);
            }
        }

        private void Neu_Click(object sender, RoutedEventArgs e)
        {
            m_dlg = new startdlg();

            m_dlg.spieler1.Text = m_name1;
            m_dlg.spieler2.Text = m_name2;

            if ((bool)m_dlg.ShowDialog())
            {
                m_name1 = m_dlg.m_name1;
                m_name2 = m_dlg.m_name2;

                resetScoreEtc();

                // mit new wären die alten objekte weiterhin sichtbar aber nicht dragable ==> besser: nur Attribute  der alten Objekte ändern 
                m_balls[0].init(350, 800, m_dlg.m_radius, 0.0005, System.Drawing.Color.White);
                m_balls[1].init(280, 220, m_dlg.m_radius, 0.0005, System.Drawing.Color.Yellow);
                m_balls[2].init(420, 220, m_dlg.m_radius, 0.0005, System.Drawing.Color.Red);

                m_cue.init(420, 800, Math.PI / 4, 140, 1000);

                radius.Value = m_dlg.m_radius;
                impulse.Value = 1000;
            }
        }

        private void Ende_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void radius_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!(m_balls[0].distance(m_balls[1]) <= 2 * radius.Value ||
                 m_balls[0].distance(m_balls[2]) <= 2 * radius.Value ||
                 m_balls[1].distance(m_balls[2]) <= 2 * radius.Value))
            {
                foreach (ball ball in m_balls)
                {
                    ball.m_radius = radius.Value;
                    label1.Content = (int)radius.Value + " px";
                }
            }
        }

        private void impulse_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            m_cue.m_impulse = impulse.Value;
            label2.Content = (int)impulse.Value + " px/s"; 
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
            {
                m_takeaim = true;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {           
            m_takeaim = false;
        }

        private void impulse_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            impulse.Value += e.Delta/120;
            m_cue.m_impulse = impulse.Value;
            label2.Content = (int)impulse.Value + " px/s"; 
        }

        private void radius_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!(m_balls[0].distance(m_balls[1]) <= 2 * radius.Value ||
                 m_balls[0].distance(m_balls[2]) <= 2 * radius.Value ||
                 m_balls[1].distance(m_balls[2]) <= 2 * radius.Value))
            {
                radius.Value += e.Delta/120;

                foreach (ball ball in m_balls)
                {
                    ball.m_radius = radius.Value;
                    label1.Content = (int)radius.Value + " px";
                }
            }
        }
    }
}
