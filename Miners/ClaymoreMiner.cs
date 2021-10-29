﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GolemUI.Interfaces;
using Microsoft.Extensions.Logging;

namespace GolemUI.Miners
{
    public class ClaymoreMiner : IMinerApp
    {
        private MinerAppName _minerAppName;
        public MinerAppName MinerAppName => _minerAppName;

        private ClaymoreParser _phoenixParser;
        private ILogger? _logger;

        public ClaymoreMiner(ILogger? logger)
        {
            _logger = logger;
            _minerAppName = new MinerAppName(MinerAppName.MinerAppEnum.Claymore);
            _phoenixParser = new ClaymoreParser(true, 5, _logger);
        }


        public string WorkingDir => @"plugins\claymore";

        public string ExePath => @"plugins\claymore\EthDcrMiner64.exe";

        public string PreBenchmarkParams => "";

        public string GetBenchmarkParams(string pool, string ethereumAddress, string nodeName)
        {
            return $"-wd 0 -r -1 -epool {pool} -ewal {ethereumAddress} -eworker \"benchmark:0x0/{nodeName}:{ethereumAddress}/0\" -clnew 1 -clKernel 0";
        }
        
        public IMinerParser MinerParser => _phoenixParser;
    }
}
