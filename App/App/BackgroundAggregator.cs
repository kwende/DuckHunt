using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckHunt
{
    public class BackgroundAggregator
    {
        private int _maxBackgroundDistance = 0; 
        public BackgroundAggregator(int maxBackgroundDistance)
        {
            _maxBackgroundDistance = maxBackgroundDistance; 
        }

        public List<DepthFrame> _frames = new List<DepthFrame>();

        public DepthFrame GetBackground(DepthFrame currentFrame)
        {
            DepthFrame ret = null; 
            _frames.Add(currentFrame);

            if (_frames.Count == 30 * 5 + 1)
            {
                _frames.RemoveAt(0);

                ret = DepthFrame.FromNumbers(currentFrame.Width, currentFrame.Height);

                for (int y = 0, i = 0; y < ret.Height; y++)
                {
                    for (int x = 0; x < ret.Width; x++, i++)
                    {
                        short farthest = 0; 

                        for (int f = 0; f < _frames.Count; f++)
                        {
                            short v = _frames[f].RawData[i]; 

                            if(v != 0 && v > farthest)
                            {
                                farthest = v; 
                            }
                        }

                        ret.RawData[i] = farthest > _maxBackgroundDistance ? (short)0 : farthest; 
                    }
                }
            }

            return ret; 
        }
    }
}
