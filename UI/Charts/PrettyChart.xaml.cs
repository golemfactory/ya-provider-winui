using GolemUI.Model;
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

namespace GolemUI.UI.Charts
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class PrettyChart : UserControl
    {
        public PrettyChart()
        {
            InitializeComponent();
            DrawChart();


        }

        public PrettyChartData ChartData
        {
            get { return (PrettyChartData)GetValue(_chartData); }
            set { SetValue(_chartData, value); }
        }

        public static readonly DependencyProperty _chartData =
            DependencyProperty.Register("ChartData", typeof(PrettyChartData), typeof(PrettyChart), new UIPropertyMetadata(new PrettyChartData()));


        void DrawChart()
        {
            var cd = ChartData;

            {
                PointCollection myPointCollection = new PointCollection();

                if (cd.BinData.BinEntries.Count > 0)
                {
                    myPointCollection.Add(new Point(0, 0));
                    myPointCollection.Add(new Point(0, 1));
                    myPointCollection.Add(new Point(1, 1));
                }
                else
                {
                    myPointCollection.Add(new Point(0, 0));
                    myPointCollection.Add(new Point(0, 1));
                    myPointCollection.Add(new Point(1, 1));
                    myPointCollection.Add(new Point(1, 0));
                }

                Polygon myPolygon = new Polygon();
                myPolygon.Points = myPointCollection;
                myPolygon.Fill = Brushes.Blue;
                myPolygon.Width = 100;
                myPolygon.Height = 100;
                myPolygon.Stretch = Stretch.Fill;
                myPolygon.Stroke = Brushes.Black;
                myPolygon.StrokeThickness = 2;

                cv.Children.Add(myPolygon);

            }
            
            for (int i = 0; i < cd.BinData.BinEntries.Count; i++)
            {
                double val = cd.BinData.BinEntries[i].Value;

                PointCollection myPointCollection = new PointCollection();
                myPointCollection.Add(new Point(0, 0));
                myPointCollection.Add(new Point(0, 1));
                myPointCollection.Add(new Point(1, 1));
                myPointCollection.Add(new Point(1, 0));

                Polygon myPolygon = new Polygon();
                myPolygon.Points = myPointCollection;
                myPolygon.Fill = Brushes.Blue;
                myPolygon.Width = 10;
                myPolygon.Height = 10;
                myPolygon.Stretch = Stretch.Fill;
                myPolygon.Stroke = Brushes.Black;
                myPolygon.StrokeThickness = 2;

                SetPosition(myPolygon, i * 10, val);

                cv.Children.Add(myPolygon);
            }
           

        }

        private static void SetPosition(DependencyObject obj, double x, double y)
        {
            obj.SetValue(Canvas.LeftProperty, x);
            obj.SetValue(Canvas.TopProperty, y);
        }
    }
}
