using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CS2HeadAimbotTutorial
{
    public class Entity
    {
        public IntPtr pawnAddress { get; set; }
        public IntPtr controllerAddress { get; set; }
        public Vector3 origin { get; set; }
        public Vector3 view { get; set; }
        public Vector3 head {  get; set; }
        public int health { get; set; }
        public int team { get; set; }
        public uint lifeState { get; set; }
        public float distance { get; set; }
    }
}
