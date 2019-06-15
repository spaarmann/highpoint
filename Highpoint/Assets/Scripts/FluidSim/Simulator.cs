namespace Highpoint {
    public class Simulator {
        private Fluid fluid;
        private float timeStep;

        public Simulator(Fluid f, float timeStep) {
            fluid = f;
            this.timeStep = timeStep;
        }

        public void SimulateStep() {

        }
    }
}