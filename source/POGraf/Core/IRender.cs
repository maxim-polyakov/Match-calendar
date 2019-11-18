using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public interface IRender
    {
        void RenderTo(Answer answer);
    }

    public class ConsoleRender : IRender
    {
        public void RenderTo(Answer answer)
        {
            Console.WriteLine("answer");
        }
    }
}
