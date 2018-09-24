using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace ConvertGyroFile {
	public partial class Form1 : Form {
		FilterV3 f = new FilterV3();

		public Form1() {
			InitializeComponent();
			f.setEcartMax(0.05);
			f.setCalibrateDuration(6000);
			f.setPercentGood(90);
		}

		private void button1_Click(object sender, EventArgs e) {
			folderBrowserDialog1.SelectedPath = textBox1.Text;
			if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
				textBox1.Text = folderBrowserDialog1.SelectedPath;
		}

		private void button2_Click(object sender, EventArgs e) {
			foreach (string srcFile in Directory.GetFiles(textBox1.Text, "*.txt"))
				if (chkCat.Checked)
					convert(srcFile, textBox1.Text + "\\full.csv");
				else {
					string dstFile = srcFile.Substring(0,srcFile.Length-4) + ".csv";
					if (!File.Exists(dstFile))
						convert(srcFile, dstFile);
				}
		}

		private void button3_Click(object sender, EventArgs e) {
			File.Delete("C:\\Users\\vdubourg\\Desktop\\MesureGyro\\Cycle\\20170609141515.csv");
			convert("C:\\Users\\vdubourg\\Desktop\\MesureGyro\\Cycle\\20170609141515.txt",
					"C:\\Users\\vdubourg\\Desktop\\MesureGyro\\Cycle\\20170609141515.csv");
		}

		private double calc(byte bH, byte bL, double div) {
			return (double)((short)(bH << 8 | bL)) / div;
		}

		private void convert(string srcFile, string dstFile) {
			//Filter f = new Filter();
			//f.setAvgSize(100);
			//f.setEcartMax(8);
			//f.setCalibrateDuration(2000);
			//f.setPercentGood(70);

			if (File.Exists(srcFile)) {
				using (BinaryReader reader = new BinaryReader(File.Open(srcFile, FileMode.Open))) {
					using (StreamWriter writer = new StreamWriter(dstFile,true)) {
						byte[] resByte = new byte[10];
						writer.WriteLine("vX;vY;vZ;vAX;vAY;vAZ;rX;rY;rZ;Good;CntG;CntB;Percent;State");
						while (reader.BaseStream.Position != reader.BaseStream.Length) {
							byte bRead = reader.ReadByte();
							if (bRead == 0x55) {
								reader.Read(resByte, 0, resByte.Length);
								if (resByte[0] == 0x52) {
									//for (int axe = 0; axe < 3; axe++)
									//	f.addValue(axe,(short)(resByte[axe * 2 + 2] << 8 | resByte[axe * 2 + 1]));
									f.addValue( (short)(resByte[2] << 8 | resByte[1]), 
												(short)(resByte[4] << 8 | resByte[3]),
												(short)(resByte[6] << 8 | resByte[5]));

//									string temp = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8}", f.getValue(0), f.getValue(1), f.getValue(2), f.getValueF(0), f.getMoyenne(0), f.getValueF(1), f.getMoyenne(1), f.getValueF(2), f.getMoyenne(2));
									//string temp = string.Format("{0};{1};{2};{4};{6};{8}", f.getValue(0), f.getValue(1), f.getValue(2), f.getValueF(0), f.getSens(0), f.getValueF(1), f.getSens(1), f.getValueF(2), f.getSens(2));
									string temp = string.Format("{0};{1};{2};{3:0.0000};{4:0.0000};{5:0.0000};{6:0.0000};{7:0.0000};{8:0.0000};{9};{10};{11};{12};{13}", 
														f.getLastValue(0), f.getLastValue(1), f.getLastValue(2),
														f.getLastValueDouble(0), f.getLastValueDouble(1), f.getLastValueDouble(2),
														f.getResult(0), f.getResult(1), f.getResult(2),
														//f.getMoyenne(0), f.getMoyenne(1), f.getMoyenne(2),
														f.getGood(), f.getCountGood(), f.getCountBad(), 
														f.getGoodPercent(),	f.getCalibrationState());
									writer.WriteLine(temp);
								}
							}
						}
					}
				}
			}
		}

		private void test(string dstFile) {
			Filter f = new Filter();
			f.setAvgSize(20);

			using (StreamWriter writer = new StreamWriter(dstFile, true)) {
				for (int i = 0; i<200 ; i++){
					if (i < 100 && i>90)
						f.addValue((short)100 ,(short)100, (short)100 );
					else
						f.addValue((short)0, (short)0, (short)0);

//					string temp = string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8}", f.getValue(0), f.getValue(1), f.getValue(2), f.getValueF(0), f.getMoyenne(0), f.getValueF(1), f.getMoyenne(1), f.getValueF(2), f.getMoyenne(2));
					//writer.WriteLine(temp);
				}
			}
		}

		//double v1 = (double)pile[index, 0] * 2000 / 32768;
	}
}
