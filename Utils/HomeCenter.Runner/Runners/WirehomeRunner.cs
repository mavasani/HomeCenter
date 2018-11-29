﻿using SimpleInjector;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    public class WirehomeRunner : Runner
    {
        private readonly List<Runner> _runners = new List<Runner>();

        public WirehomeRunner(List<Runner> runners) : base(nameof(WirehomeRunner), runners.Select(r => r.GetType().Name).ToArray())
        {
            _runners = runners;
        }

        public async Task Initialize()
        {
            var container = new Container();
            var bootstrapper = new WirehomeBootstrapper(container, $@"..\..\..\..\..\Configurations\componentConfiguration.json");
            var controller = await bootstrapper.BuildController().ConfigureAwait(false);

            foreach (var runner in _runners)
            {
                runner.SetContainer(container);
            }

            await Task.Delay(1000).ConfigureAwait(false);
        }

        public override Task RunTask(int taskId)
        {
            return _runners[taskId].Run();
        }
     
    }
}