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
    public class ball
    {       
        public double m_frict, m_x, m_y, m_xold, m_yold, m_vx, m_vy, m_radius;
        public bool m_isDragged;
        public Ellipse m_theball = new Ellipse();

        public ball(double x = 100, double y = 100, double radius = 20, double frict = 0.0005, System.Drawing.Color? color = null)
        {
           init(x, y, radius, frict, color);
        }

        public void init(double x = 100, double y = 100, double radius = 20, double frict = 0.0005, System.Drawing.Color? color = null)
        {
            m_isDragged = false;
            m_vx = 0.0;
            m_vy = 0.0;
            m_frict = frict;
            m_x = m_xold = x;
            m_y = m_yold = y;
            m_radius = radius;

            if (color == null)
            {
                m_theball.Fill = System.Windows.Media.Brushes.Red;
            }
            else
            {
                System.Drawing.Color c = (System.Drawing.Color)color;

                RadialGradientBrush brush = new RadialGradientBrush();
                brush.GradientOrigin = new System.Windows.Point(0.75, 0.25);
                brush.GradientStops.Add(new GradientStop(Colors.White, 0.0));
                brush.GradientStops.Add(new GradientStop(Colors.Beige, 0.2));
                brush.GradientStops.Add(new GradientStop(System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B), 1.0));
      
                m_theball.Fill = brush;
            }

            m_theball.Cursor = Cursors.Hand;

            setLTWH();
        }

        public void drag(double px, double py, cue c)
        {
            if (!c.m_isDragged)
            {
                m_vx = m_vy = 0;

                m_y = px;
                m_x = py;

                setLTWH();
            }
        }

        public bool PtInBall(double px, double py)
        {
            if (Math.Sqrt((px-m_x)*(px-m_x)+(py-m_y)*(py-m_y)) < m_radius)
            {
                return true;
            }
            return false;
        }
        
        public void setLTWH()
        {
            Canvas.SetLeft(m_theball, m_y - m_radius);
            Canvas.SetTop(m_theball, m_x - m_radius);
            m_theball.Width = 2 * m_radius;
            m_theball.Height = 2 * m_radius;
        }

        public void move(double dt)
        {
            m_xold = m_x;
            m_yold = m_y;

            m_x += m_vx * dt / 1000;
            m_y += m_vy * dt / 1000;

            setLTWH();
        }

        public void friction(double dt)
        {
            if (Math.Sqrt(m_vx * m_vx + m_vy * m_vy) <= 5)
            {
                m_vx = m_vy = 0;
            }
            else
            {
                m_vx *= (1 - m_frict * dt);
                m_vy *= (1 - m_frict * dt);
            }
        }

        public void collision(System.Windows.Shapes.Rectangle table)
        {
            if (m_x - m_radius <= Canvas.GetTop(table))
            {
                m_vx = -m_vx;
                m_x += 2 * (Canvas.GetTop(table) - (m_x - m_radius));
            }
            else if (m_x + m_radius >= Canvas.GetTop(table) + table.Height)
            {
                m_vx = -m_vx;
                m_x -= 2 * (m_x + m_radius - Canvas.GetTop(table) - table.Height);
            }

            if (m_y - m_radius <= Canvas.GetLeft(table))
            {
                m_vy = -m_vy;
                m_y += 2 * (Canvas.GetLeft(table) - (m_y - m_radius));
            }
            else if (m_y + m_radius >= Canvas.GetLeft(table) + table.Width)
            {
                m_vy = -m_vy;
                m_y -= 2 * (m_y + m_radius - Canvas.GetLeft(table) - table.Width);
            }

            setLTWH();
        }

        public double distance(ball b)
        {
            return Math.Sqrt((m_x - b.m_x) * (m_x - b.m_x) + (m_y - b.m_y) * (m_y - b.m_y));
        }

        public bool collision(ball ball, uint dt)
        {
            bool hit = false;

            if (distance(ball) <= 2 * m_radius)
            {
                hit = true;

                double tk;

                // Vorbereitung der Positionskorrektur
                double a = ((m_y - m_yold) - (ball.m_y - ball.m_yold)) * ((m_y - m_yold) - (ball.m_y - ball.m_yold)) +
                           ((m_x - m_xold) - (ball.m_x - ball.m_xold)) * ((m_x - m_xold) - (ball.m_x - ball.m_xold));

                double b = (2 * (m_yold - ball.m_yold) * ((m_y - m_yold) - (ball.m_y - ball.m_yold))) +
                           (2 * (m_xold - ball.m_xold) * ((m_x - m_xold) - (ball.m_x - ball.m_xold)));

                double c = ((m_xold - ball.m_xold) * (m_xold - ball.m_xold)) +
                           ((m_yold - ball.m_yold) * (m_yold - ball.m_yold)) -
                           (4 * (m_radius * m_radius));

                double p = b / a;
                double q = c / a;

                double tk1 = -(p / 2) + Math.Sqrt(((p / 2) * (p / 2)) - q);
                double tk2 = -(p / 2) - Math.Sqrt(((p / 2) * (p / 2)) - q);

                if (tk1 >= 0 && tk1 <= 1)
                {
                    tk = tk1;
                }
                else
                {
                    tk = tk2;
                }

                // Positionskorrektur, so dass sich die Kugeln gerade berühren	
                double x1 = m_xold + (m_x - m_xold) * tk;
                double x2 = ball.m_xold + (ball.m_x - ball.m_xold) * tk;
                double y1 = m_yold + (m_y - m_yold) * tk;
                double y2 = ball.m_yold + (ball.m_y - ball.m_yold) * tk;

                // Winkelfunktionen für die Koordinatentransformation (muss mit den korrigierten
                // Werten berechnet werden)
                double cos_a = (x2 - x1) / (2 * m_radius);
                double sin_a = (y2 - y1) / (2 * m_radius);

                // Koordinatentransformation der Geschwindigkeitsvektoren
                double am_vx = cos_a * m_vx + sin_a * m_vy;
                double am_vy = -sin_a * m_vx + cos_a * m_vy;

                double bm_vx = cos_a * ball.m_vx + sin_a * ball.m_vy;
                double bm_vy = -sin_a * ball.m_vx + cos_a * ball.m_vy;

                // Zentralkomponenten des Impulses austauschen	
                double save = bm_vx;
                bm_vx = am_vx;
                am_vx = save;

                // Rücktransformation
                m_vx = cos_a * am_vx - sin_a * am_vy;
                m_vy = sin_a * am_vx + cos_a * am_vy;

                ball.m_vx = cos_a * bm_vx - sin_a * bm_vy;
                ball.m_vy = sin_a * bm_vx + cos_a * bm_vy;

                // Endgültige Positionskorrektur mit rücktransformiertem Vektor
                m_x = x1 + m_vx * dt * (1 - tk) / 1000;
                m_y = y1 + m_vy * dt * (1 - tk) / 1000;

                ball.m_x = x2 + ball.m_vx * dt * (1 - tk) / 1000;
                ball.m_y = y2 + ball.m_vy * dt * (1 - tk) / 1000;

                setLTWH();
            }

            return hit;
        }
    }
}
