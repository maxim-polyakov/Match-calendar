using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest
{
    public class Sample
    {
        public static Model Generate()
        {
            Model model = new Model();
            model.n = 8;
            model.nl = 4;
            model.s = new int[] { 3, 4, 8 };
            model.d = 23;
            model.f = 2;
            model.g = 3;

            model.q = new int[] { 0, 1, 0, 0, 0, 4, 1, 0, 0, 2, 0, 4, 0, 3, 2, 0, 1, 2, 3, 4, 1, 2, 3 };
            model.w = new int[] { 0, 0, 1, 2, 3, 3, 4, 5, 6, 6, 7, 7, 8, 8, 9, 10, 10, 10, 10, 10, 11, 11, 11 };

            model.t = new int[model.n][];
            model.t[0] = new int[] { 1, 2 };
            model.t[1] = new int[] { 1 };
            model.t[2] = new int[] { 0 };
            model.t[3] = new int[] { 0, 2 };
            model.t[4] = new int[] { 0 };
            model.t[5] = new int[] { 1 };
            model.t[6] = new int[] { 1 };
            model.t[7] = new int[] { 2 };

            model.v = new V[model.n][];
            model.v[0] = new V[] { new V(1, 3) };
            model.v[1] = new V[] { new V(1, 2), new V(4, 2) };
            model.v[2] = new V[] { new V(3, 2) };
            model.v[3] = new V[0];
            model.v[4] = new V[0];
            model.v[5] = new V[] { new V(1, 3) };
            model.v[6] = new V[] { new V(1, 2), new V(4, 2) };
            model.v[7] = new V[] { new V(1, 2) };

            return model;
        }
    }
}
