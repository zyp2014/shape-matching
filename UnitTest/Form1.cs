using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using ShapeContext;
using LiniarAlgebra;
using PCA;
using Adaption;

namespace UnitTest
{
    public partial class Form1 : Form
    {
        public static readonly int r_DefaultNumOfSamples = 50;

        private Point[] m_shape1Points;
        private Point[] m_shape2Points;

        private Graphics m_graphics;
        public Form1()
        {
            InitializeComponent();

            Bitmap surface = new Bitmap(sketch.Width, sketch.Height);
            Graphics.FromImage(surface).Clear(Color.WhiteSmoke);
            sketch.Image = surface;

            m_graphics = Graphics.FromImage(sketch.Image);

            m_shape1Points = prepareDemoRect1();
            m_shape2Points = prepareDemoRect2();

            Pen dpen = new Pen(Color.Black,1);

            drawPolygon(m_shape1Points,m_graphics,dpen);
            dpen.Color = Color.Red;
            drawPolygon(m_shape2Points,m_graphics,dpen);
        }

        private void drawPolygon(Point[] i_shapePoints,Graphics i_Graphics,Pen i_drawingPen)
        {
            if (i_shapePoints.Length < ShapeContext.Utils.sr_MinPointsForPolygon)
            {//Polygon must contain at least three points
                return;
            }
            
            i_Graphics.DrawPolygon(i_drawingPen, i_shapePoints);
        }

        private Point[] prepareDemoRect2()
        {
            // original
            Point[] retOriginPoints = {
                new Point(230,80),    // top left
                new Point(310,80),    // top right
                new Point(310,210),   // bottom right
                new Point(230,210) }; // bottom left

            return retOriginPoints;
        }

        private Point[] prepareDemoRect1()
        {
            // original
            Point[] retOriginPoints = {
                new Point(200,50),    // top left
                new Point(300,50),    // top right
                new Point(300,200),   // bottom right
                new Point(200,200) }; // bottom left

            return retOriginPoints;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Point[] shape1samples = ShapeContext.Utils.GetSamplePointsFromPath(m_shape1Points, r_DefaultNumOfSamples, true);
            Point[] shape2samples = ShapeContext.Utils.GetSamplePointsFromPath(m_shape2Points, r_DefaultNumOfSamples, true);

            //ShapeContextMatching matching = new ShapeContextMatching(shape1samples, shape2samples, null);
            //int[] matches = matching.FindMatches();

            //drawMathces(shape1samples, shape2samples,matches, new Pen(Color.Green, 1));
        }

        private void drawMathces(Point[] i_shape1samples,Point[] i_shape2samples,int[] i_matches,Pen i_DrawingPen)
        {
            for (int i = 0; i < i_matches.Length; ++i)
            {
                m_graphics.DrawLine(i_DrawingPen, i_shape1samples[i], i_shape2samples[i_matches[i]]);
            }

            sketch.Refresh();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DoubleMatrix test = new DoubleMatrix(2, 10);
            test[0, 0] = 2.5;
            test[0, 1] = 0.5;
            test[0, 2] = 2.2;
            test[0, 3] = 1.9;
            test[0, 4] = 3.1;
            test[0, 5] = 2.3;
            test[0, 6] = 2.0;
            test[0, 7] = 1.0;
            test[0, 8] = 1.5;
            test[0, 9] = 1.1;

            test[1, 0] = 2.4;
            test[1, 1] = 0.7;
            test[1, 2] = 2.9;
            test[1, 3] = 2.2;
            test[1, 4] = 3.0;
            test[1, 5] = 2.7;
            test[1, 6] = 1.6;
            test[1, 7] = 1.1;
            test[1, 8] = 1.6;
            test[1, 9] = 0.9;
            
            PCAtransform pcaMatching = new PCAtransform(test);
            DoubleMatrix PCAmat = pcaMatching.Calculate();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            AlgoFactory factory = new AlgoFactory();
            sketch.Image = factory.GetAlgo(AlgoFactory.ShapeContext).Run(
                Image.FromFile(@"..\..\..\fish_shape_b.bmp"),
                Image.FromFile(@"..\..\..\fish_shape_a.bmp"), null).ResultImage;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            AlgoFactory factory = new AlgoFactory();
            sketch.Image = factory.GetAlgo(AlgoFactory.PCA).Run(
                Image.FromFile(@"..\..\..\Example - 1.png"),
                Image.FromFile(@"..\..\..\Example - 2.png"), null).ResultImage;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            AlgoFactory factory = new AlgoFactory();
            IMatchingAlgo algo = factory.GetAlgo(AlgoFactory.Hausdorff);
            algo.Create(
                Image.FromFile(@"..\..\..\Example - 1.png"),
                Image.FromFile(@"..\..\..\Example - 2.png"));
            sketch.Image = algo.Run().ResultImage;
        }


    }
}
