using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;

namespace WPFBlrd
{
    public class cue
    {
        public double m_x, m_y, m_length, m_angle, m_impulse;
        public bool m_isDragged;
        public Ellipse m_knob = new Ellipse();
        public Line m_line = new Line();
        public Line m_laser = new Line();

        public cue(double x = 420, double y = 800, double angle = Math.PI/4, int length = 140, double impulse = 1000)
        {
            init(x, y, angle, length, impulse);
        }

        public void init(double x = 420, double y = 800, double angle = Math.PI/4, int length = 140, double impulse = 1000)
        {
            m_x = x;
            m_y = y;
            m_length = length;
            m_angle = angle;
            m_impulse = impulse;
            m_isDragged = false;

            m_line.Stroke = System.Windows.Media.Brushes.Black;
            m_line.StrokeThickness = 2;
            m_laser.Stroke = System.Windows.Media.Brushes.Red;
            m_laser.StrokeThickness = 1;

            m_line.X1 = m_y + (m_length / 2) * Math.Cos(m_angle);
            m_line.Y1 = m_x + (m_length / 2) * Math.Sin(m_angle);
            m_line.X2 = m_y - (m_length / 2) * Math.Cos(m_angle);
            m_line.Y2 = m_x - (m_length / 2) * Math.Sin(m_angle);

            Canvas.SetLeft(m_knob, m_y + (m_length / 2) * Math.Cos(m_angle) - 5);
            Canvas.SetTop(m_knob, m_x + (m_length / 2) * Math.Sin(m_angle) - 5);

            m_knob.Width = 10;
            m_knob.Height = 10;

            m_knob.Fill = System.Windows.Media.Brushes.Blue;
            m_knob.Cursor = Cursors.Hand;
        }

        public void drag(double px, double py, ball b)
        {
            double x = py;
            double y = px;

            m_x = x - (m_length / 2) * (x - b.m_x) / Math.Sqrt((y - b.m_y) * (y - b.m_y) + (x - b.m_x) * (x - b.m_x));
            m_y = y - (m_length / 2) * (y - b.m_y) / Math.Sqrt((y - b.m_y) * (y - b.m_y) + (x - b.m_x) * (x - b.m_x));

            double geg = x - b.m_x;
            double ank = y - b.m_y;
            double quot = ank / Math.Sqrt(ank * ank + geg * geg);

            if (geg < 0)
            {
                m_angle = 2 * Math.PI - Math.Acos(quot);
            }
            else
            {
                m_angle = Math.Acos(quot);
            }

            m_line.X1 = m_y + (m_length / 2) * Math.Cos(m_angle);
            m_line.Y1 = m_x + (m_length / 2) * Math.Sin(m_angle);
            m_line.X2 = m_y - (m_length / 2) * Math.Cos(m_angle);
            m_line.Y2 = m_x - (m_length / 2) * Math.Sin(m_angle);

            Canvas.SetLeft(m_knob, m_y + (m_length / 2) * Math.Cos(m_angle) - 5);
            Canvas.SetTop(m_knob, m_x + (m_length / 2) * Math.Sin(m_angle) - 5);
        }

        public void laser(bool takeaim)
        {
            if (takeaim)
            {
                m_laser.X1 = m_y - (0.5 * m_length) * Math.Cos(m_angle);
                m_laser.Y1 = m_x - (0.5 * m_length) * Math.Sin(m_angle);
                m_laser.X2 = m_y - (5 * m_length) * Math.Cos(m_angle);
                m_laser.Y2 = m_x - (5 * m_length) * Math.Sin(m_angle);
            }
            else
            {
                m_laser.X1 = m_laser.X2 = m_laser.Y1 = m_laser.Y2 = 0;
            }
        }

        public bool PtInKnob(double px, double py)
        {
            double left = Canvas.GetLeft(m_knob);
            double top = Canvas.GetTop(m_knob);

            if (Math.Sqrt((px - top - 5)*(px - top - 5) + (py - left - 5)*(py - left - 5)) < 5)
            {
                return true;
            }
            return false;
        }

        public void shoot(ball b)
        {
            double tip_y = m_y - (m_length / 2) * Math.Cos(m_angle);
            double tip_x = m_x - (m_length / 2) * Math.Sin(m_angle);

            double dist = Math.Sqrt((b.m_x - tip_x) * (b.m_x - tip_x) + (b.m_y - tip_y) * (b.m_y - tip_y));

            if (dist > b.m_radius && dist < 2.0 * b.m_radius)
            {
                b.m_vx = -m_impulse * Math.Sin(m_angle);
                b.m_vy = -m_impulse * Math.Cos(m_angle);
            }
        }
    }
}
