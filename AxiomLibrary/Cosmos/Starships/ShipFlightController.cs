using Axiom.Cosmos.Starships;
using System;

namespace Assets.AxiomCore.Cosmos_Link.Starships
{
    public class ShipFlightController
    {
        #region Fields
        private readonly Starship _ship;
        // Stato interno: accumulano i radianti totali
        private double _internalPitch = 0;
        private double _internalYaw = 0;
        private double _internalRoll = 0;

        // Velocità di rotazione attuali (radianti al secondo)
        private double _pitchVelocity = 0;
        private double _yawVelocity = 0;
        private double _rollVelocity = 0;

        // Coefficiente di smorzamento (0.0 = inerzia infinita, 1.0 = stop istantaneo)
        public double AngularDamping { get; set; } = 5.0;
        #endregion

        #region Properties
        /// <summary>
        /// Sensibilità della rotazione della nave.
        /// </summary>
        public double RotationSensitivity { get; set; } = 1.5;
        /// <summary>
        /// sensibilità della variazione del thrust (potenza motori).
        /// </summary>
        public double ThrustSensitivity { get; set; } = 0.05;
        #endregion

        #region Constructors
        public ShipFlightController(Starship ship)
        {
            _ship = ship;
            // Sincronizziamo lo stato iniziale con la rotazione attuale della nave
            _ship.GetRotation(out _internalPitch, out _internalYaw, out _internalRoll);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Aggiorna lo stato della nave in base agli input.
        /// Valori attesi tra -1.0 e 1.0 per gli assi, e 0.0-1.0 per il throttle.
        /// </summary>
        /// <param name="pitch">Inclinazione avanti/indietro</param>
        /// <param name="roll">Inclinazione laterale</param>
        /// <param name="yaw">Rotazione sinistra/destra</param>
        /// <param name="throttleDelta">delta per la potenza</param>
        /// <param name="dt">Parametro di tempo</param>
        public void HandleInput(double pitch, double yaw, double roll, double throttleDelta, double dt)
        {
            // 1. Applichiamo l'input alle velocità (Accelerazione angolare)
            if (Math.Abs(pitch) > 0.001)
                _pitchVelocity = pitch * RotationSensitivity;
            else _pitchVelocity = Lerp(_pitchVelocity, 0, AngularDamping * dt); // Damping

            if (Math.Abs(yaw) > 0.001)
                _yawVelocity = yaw * RotationSensitivity;
            else _yawVelocity = Lerp(_yawVelocity, 0, AngularDamping * dt); // Damping

            if (Math.Abs(roll) > 0.001) _rollVelocity = roll * RotationSensitivity;
            else _rollVelocity = Lerp(_rollVelocity, 0, AngularDamping * dt); // Damping

            // 2. Aggiorniamo gli angoli interni usando le velocità
            _internalPitch += _pitchVelocity * dt;
            _internalYaw += _yawVelocity * dt;
            _internalRoll += _rollVelocity * dt;

            // 3. Applichiamo la rotazione alla nave
            _ship.SetRotation(_internalPitch, _internalYaw, _internalRoll);

            // 4. Gestione Throttle
            double input = throttleDelta;
            if (input > 0)
            {
                // Incremento normale
                _ship.CurrentThrottle = Math.Clamp(_ship.CurrentThrottle + (throttleDelta * ThrustSensitivity * dt), 0.0, 1.0);
            }
            else
            {
                // Decadimento automatico verso lo zero (es. perde il 50% al secondo)
                _ship.CurrentThrottle = Math.Max(0.0, _ship.CurrentThrottle - (2.0 * dt));
            }
            //_planet.CurrentThrottle = Math.Clamp(_planet.CurrentThrottle + (throttleDelta * ThrustSensitivity * dt), 0.0, 1.0);

            // 5. Update della matrice per la fisica (ZVector/Forward)
            _ship.UpdateRTMatrix();
        }


        private double Lerp(double start, double end, double t)
        {
            return start + (end - start) * Math.Clamp(t, 0, 1);
        }



        #endregion
    }
}
