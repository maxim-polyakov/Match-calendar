using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public interface IRender
    {
        void RenderTo(IAnswer answer);
    }

    public class ConsoleRender : IRender
    {
        public void RenderTo(IAnswer answer)
        {
            Console.WriteLine("answer");
        }
    }
}
