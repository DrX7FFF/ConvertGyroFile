using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertGyroFile {
	class Filter {
		Phase mode = Phase.calibrating;
		int index = 0;
		short[,] pile = new short[1,3];

		uint countGood = 0;
		uint countBad = 0;

		int[] somme = new int[3];
		short[] moyenne = new short[3];
		short[] sens = new short[3];
		double[] result = new double[3];
		double[] somme2 = new double[3];
		double[] lastValueDouble = new double[3];
		short[] lastValue = new short[3];
		bool good = false;

		int avgSize = 1;
		short ecartMax = 10;
		double ecartMaxDouble = 0.05;
		uint calibrateDuration = 3000;  //30 sec
		ushort percentGood = 95;

		public void setEcartMax(short ecartMax) {
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

		public void setAvgSize(int avgSize) {
			if (avgSize <= 1) 
				return;

			this.avgSize = avgSize;
			pile = new short[this.avgSize, 3];
			for (int i = 0; i < 3; i++) {
				somme[i] = 0;
				moyenne[i] = 0;
				sens[0] = 0;
			}
			index = avgSize - 1;
			countGood = 0;
			countBad = 0;
		}

		public Filter() {
		}

		public void addValue(short val0, short val1, short val2) {
			double vitNorm = Math.Sqrt(val0 * val0 + val1 * val1 + val2 * val2);

			lastValue[0] = val0;
			lastValue[1] = val1;
			lastValue[2] = val2;

			good = true;
			for (int i = 0; i < 3; i++) {
				if (vitNorm == 0)
					lastValueDouble[i] = 0;
				else
					lastValueDouble[i] = lastValue[i] / vitNorm;
				good = good && (Math.Abs(lastValueDouble[i] - result[i]) <= ecartMaxDouble);
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


		//public void addValue(short val0, short val1, short val2) {
		//	index++;
		//	if (index == avgSize)
		//		index = 0;

		//	for (int i = 0; i < 3; i++)
		//		somme[i] -= pile[index, i];

		//	pile[index, 0] = val0;
		//	pile[index, 1] = val1;
		//	pile[index, 2] = val2;

		//	good = true;
		//	for (int i = 0; i < 3; i++) {
		//		somme[i] += pile[index, i];
		//		moyenne[i] = (short)(somme[i] / avgSize);
		//		good = good && (Math.Abs(pile[index, i] - sens[i]) <= ecartMax);
		//	}
		//	if (good)
		//		countGood++;
		//	else
		//		countBad++;

		//	// TODO ajouter un controle de vitesse minimum

		//	if (getGoodPercent() < percentGood) {
		//		countGood = 0;
		//		countBad = 0;
		//		for (int i = 0; i < 3; i++)
		//			sens[i] = moyenne[i];
		//	}

		//	if (mode == Phase.calibrating) {
		//		if (countGood + countBad > calibrateDuration)
		//			mode = Phase.calibrate;
		//	}

		//}
		public short getLastValue(int axe) {
			return lastValue[axe];
		}
		public double getLastValueDouble(int axe) {
			return lastValueDouble[axe];
		}
		public double getResult(int axe) {
			return result[axe];
		}

		public short getMoyenne(int axe) {
			return moyenne[axe];
		}
		public short getSens(int axe) {
			return sens[axe];
		}
		public bool getGood() {
			return good;
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
		public bool getCalibrationState() {
			return mode == Phase.calibrate;
		}
	}
}
