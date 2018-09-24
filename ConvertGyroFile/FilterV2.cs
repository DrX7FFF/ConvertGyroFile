using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertGyroFile {
	class FilterV2 {
		Phase mode = Phase.calibrating;

		uint countGood = 0;
		uint countBad = 0;

		double[] result = new double[3];
		double[] somme2 = new double[3];
		double[] lastValueDouble = new double[3];
		short[] lastValue = new short[3];

		bool good = false;

		double ecartMax = 0.05;
		uint calibrateDuration = 3000;  //30 sec
		ushort percentGood = 95;
		ushort vitMin = 16; // x/32768*2000(°/s)

		public void setEcartMax(double ecartMax) {
			this.ecartMax = Math.Abs(ecartMax);
			countGood = 0;
			countBad = 0;
		}

		public void setCalibrateDuration(uint duration) {
			this.calibrateDuration = duration;
			countGood = 0;
			countBad = 0;
		}

		public void setPercentGood(ushort percent) {
			if (percent > 100)
				percent = 100;
			this.percentGood = percent;
			countGood = 0;
			countBad = 0;
		}

		public void addValue(short valX, short valY, short valZ) {
			double vitNorm = Math.Sqrt(valX * valX + valY * valY + valZ * valZ);

			lastValue[0] = valX;
			lastValue[1] = valY;
			lastValue[2] = valZ;

			good = true;
			for (int i = 0; i < 3; i++) {
				if (vitNorm == 0) {
					lastValueDouble[i] = 0;
					good = false;
				}
				else {
					lastValueDouble[i] = lastValue[i] / vitNorm;
					if (vitNorm < vitMin)
						good = false;
					else
						good = good && (Math.Abs(lastValueDouble[i] - result[i]) <= ecartMax);
				}
			}

			if (mode == Phase.calibrating) {
				if (good){
					countGood++;
					for (int i = 0; i < 3; i++) {
						somme2[i] += lastValueDouble[i];
						result[i] = somme2[i] / countGood;
					}
				}
				else
					countBad++;

				// TODO ajouter un controle de vitesse minimum

				if (getGoodPercent() < percentGood) {
					countGood = 1;
					countBad = 0;
					for (int i = 0; i < 3; i++) {
						somme2[i] = lastValueDouble[i]; 
						result[i] = lastValueDouble[i];
					}
				}

				if (countGood + countBad > calibrateDuration)
					mode = Phase.calibrate;
			}
		}

		public short getLastValue(int axe) {
			return lastValue[axe];
		}
		public double getLastValueDouble(int axe) {
			return lastValueDouble[axe];
		}
		public double getResult(int axe) {
			return result[axe];
		}
		public int getGood() {
			return good ? 1 : 0;
		}
		public uint getCountGood() {
			return countGood;
		}
		public uint getCountBad() {
			return countBad;
		}
		public uint getGoodPercent() {
			return countGood + countBad == 0 ? 0 : 100 * countGood / (countGood + countBad);
		}
		public int getCalibrationState() {
			return mode == Phase.calibrate ? 1 : 0;
		}
	}
}
