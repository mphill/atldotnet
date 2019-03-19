﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using ATL.AudioData;
using System.IO;
using System.Drawing;
using ATL.test.IO.MetaData;

namespace ATL.test.IO
{
    [TestClass]
    public class HighLevel
    {
        [TestMethod]
        public void TagIO_R_Single_ID3v1()
        {
            bool crossreadingDefault = MetaDataIOFactory.GetInstance().CrossReading;
            int[] tagPriorityDefault = new int[MetaDataIOFactory.TAG_TYPE_COUNT];
            MetaDataIOFactory.GetInstance().TagPriority.CopyTo(tagPriorityDefault, 0);

            /* Set options for Metadata reader behaviour - this only needs to be done once, or not at all if relying on default settings */
            MetaDataIOFactory.GetInstance().CrossReading = false;                            // default behaviour anyway
            MetaDataIOFactory.GetInstance().SetTagPriority(MetaDataIOFactory.TAG_APE, 0);    // No APEtag on sample file => should be ignored
            MetaDataIOFactory.GetInstance().SetTagPriority(MetaDataIOFactory.TAG_ID3V1, 1);  // Should be entirely read
            MetaDataIOFactory.GetInstance().SetTagPriority(MetaDataIOFactory.TAG_ID3V2, 2);  // Should not be read, since behaviour is single tag reading
            /* end set options */

            try
            {
                Track theTrack = new Track(TestUtils.GetResourceLocationRoot() + "MP3/01 - Title Screen.mp3");

                Assert.AreEqual("Nintendo Sound Scream", theTrack.Artist); // Specifically tagged like this on the ID3v1 tag
                Assert.AreEqual(0, theTrack.Year); // Specifically tagged as empty on the ID3v1 tag
            }
            finally
            {
                // Set back default settings
                MetaDataIOFactory.GetInstance().CrossReading = crossreadingDefault;
                MetaDataIOFactory.GetInstance().TagPriority = tagPriorityDefault;
            }
        }

        [TestMethod]
        public void TagIO_R_Multi()
        {
            bool crossreadingDefault = MetaDataIOFactory.GetInstance().CrossReading;
            int[] tagPriorityDefault = new int[MetaDataIOFactory.TAG_TYPE_COUNT];
            MetaDataIOFactory.GetInstance().TagPriority.CopyTo(tagPriorityDefault, 0);

            /* Set options for Metadata reader behaviour - this only needs to be done once, or not at all if relying on default settings */
            MetaDataIOFactory.GetInstance().CrossReading = true;
            MetaDataIOFactory.GetInstance().SetTagPriority(MetaDataIOFactory.TAG_APE, 0);    // No APEtag on sample file => should be ignored
            MetaDataIOFactory.GetInstance().SetTagPriority(MetaDataIOFactory.TAG_ID3V1, 1);  // Should be the main source except for the Year field (empty on ID3v1)
            MetaDataIOFactory.GetInstance().SetTagPriority(MetaDataIOFactory.TAG_ID3V2, 2);  // Should be used for the Year field (valuated on ID3v2)
            /* end set options */

            try
            {
                Track theTrack = new Track(TestUtils.GetResourceLocationRoot() + "MP3/01 - Title Screen.mp3");

                Assert.AreEqual("Nintendo Sound Scream", theTrack.Artist); // Specifically tagged like this on the ID3v1 tag
                Assert.AreEqual(1984, theTrack.Year); // Empty on the ID3v1 tag => cross-reading should read it on ID3v2
            }
            finally
            {
                // Set back default settings
                MetaDataIOFactory.GetInstance().CrossReading = crossreadingDefault;
                MetaDataIOFactory.GetInstance().TagPriority = tagPriorityDefault;
            }
        }

        [TestMethod]
        public void TagIO_R_MultiplePictures()
        {
            Track theTrack = new Track(TestUtils.GetResourceLocationRoot() + "OGG/bigPicture.ogg");

            // Check if _all_ embedded pictures are accessible from Track
            Assert.AreEqual(3, theTrack.EmbeddedPictures.Count);
        }

        [TestMethod]
        public void TagIO_RW_DeleteTag()
        {
            string testFileLocation = TestUtils.GetTempTestFile("MP3/01 - Title Screen.mp3");
            Track theTrack = new Track(testFileLocation);

            theTrack.Remove(MetaDataIOFactory.TAG_ID3V2);

            Assert.AreEqual("Nintendo Sound Scream", theTrack.Artist); // Specifically tagged like this on the ID3v1 tag
            Assert.AreEqual(0, theTrack.Year); // Empty on the ID3v1 tag => should really come empty since ID3v2 tag has been removed

            // Get rid of the working copy
            File.Delete(testFileLocation);
        }

        [TestMethod]
        public void TagIO_RW_UpdateTagBaseField()
        {
            string testFileLocation = TestUtils.GetTempTestFile("MP3/01 - Title Screen.mp3");
            Track theTrack = new Track(testFileLocation);

            theTrack.Artist = "Hey ho";
            theTrack.Year = 1944;
            theTrack.Save();

            theTrack = new Track(testFileLocation);

            Assert.AreEqual("Hey ho", theTrack.Artist);
            Assert.AreEqual(1944, theTrack.Year);

            // Get rid of the working copy
            File.Delete(testFileLocation);
        }

        [TestMethod]
        public void TagIO_RW_UpdateTagBaseField_OGG()
        {
            string testFileLocation = TestUtils.GetTempTestFile("VQF/vqf.vqf");
            Track theTrack = new Track(testFileLocation);

            theTrack.Artist = "Hey ho";
            theTrack.Year = 1944;
            theTrack.Save();

            theTrack = new Track(testFileLocation);

            Assert.AreEqual("Hey ho", theTrack.Artist);
            Assert.AreEqual(1944, theTrack.Year);

            // Get rid of the working copy
            File.Delete(testFileLocation);
        }

        [TestMethod]
        public void TagIO_RW_UpdateTagBaseField_DSD()
        {
            string testFileLocation = TestUtils.GetTempTestFile("DSF/dsf.dsf");
            Track theTrack = new Track(testFileLocation);

            theTrack.Artist = "Hey ho";
            theTrack.Year = 1944;
            theTrack.Save();

            theTrack = new Track(testFileLocation);

            Assert.AreEqual("Hey ho", theTrack.Artist);
            Assert.AreEqual(1944, theTrack.Year);

            // Get rid of the working copy
            File.Delete(testFileLocation);
        }

        [TestMethod]
        public void TagIO_RW_AddRemoveTagAdditionalField()
        {
            string testFileLocation = TestUtils.GetTempTestFile("MP3/01 - Title Screen.mp3");
            Track theTrack = new Track(testFileLocation);

            theTrack.AdditionalFields.Add("ABCD", "efgh");
            theTrack.AdditionalFields.Remove("TENC");
            theTrack.Save();

            theTrack = new Track(testFileLocation);

            Assert.AreEqual(1, theTrack.AdditionalFields.Count); // TENC should have been removed
            Assert.IsTrue(theTrack.AdditionalFields.ContainsKey("ABCD"));
            Assert.AreEqual("efgh", theTrack.AdditionalFields["ABCD"]);

            // Get rid of the working copy
            File.Delete(testFileLocation);
        }

        [TestMethod]
        public void TagIO_RW_UpdateTagAdditionalField()
        {
            string testFileLocation = TestUtils.GetTempTestFile("MP3/01 - Title Screen.mp3");
            Track theTrack = new Track(testFileLocation);

            theTrack.AdditionalFields["TENC"] = "update test";
            theTrack.Save();

            theTrack = new Track(testFileLocation);

            Assert.AreEqual(1, theTrack.AdditionalFields.Count);
            Assert.IsTrue(theTrack.AdditionalFields.ContainsKey("TENC"));
            Assert.AreEqual("update test", theTrack.AdditionalFields["TENC"]);

            // Get rid of the working copy
            File.Delete(testFileLocation);
        }

        [TestMethod]
        public void TagIO_RW_AddRemoveTagPictures()
        {
            string testFileLocation = TestUtils.GetTempTestFile("MP3/id3v2.4_UTF8.mp3");
            Track theTrack = new Track(testFileLocation);

            theTrack.EmbeddedPictures.RemoveAt(1); // Remove Conductor; Front Cover remains

            // Add CD
            PictureInfo newPicture = new PictureInfo(Commons.ImageFormat.Gif, PictureInfo.PIC_TYPE.CD);
            newPicture.PictureData = File.ReadAllBytes(TestUtils.GetResourceLocationRoot() + "_Images/pic1.gif");
            theTrack.EmbeddedPictures.Add(newPicture);

            theTrack.Save();

            theTrack = new Track(testFileLocation);

            Assert.AreEqual(2, theTrack.EmbeddedPictures.Count); // Front Cover, CD

            bool foundFront = false;
            bool foundCD = false;

            foreach (PictureInfo pic in theTrack.EmbeddedPictures)
            {
                if (pic.PicType.Equals(PictureInfo.PIC_TYPE.Front)) foundFront = true;
                if (pic.PicType.Equals(PictureInfo.PIC_TYPE.CD)) foundCD = true;
            }

            Assert.IsTrue(foundFront);
            Assert.IsTrue(foundCD);

            // Get rid of the working copy
            File.Delete(testFileLocation);
        }

        [TestMethod]
        public void TagIO_RW_UpdateTagPictures()
        {
            string testFileLocation = TestUtils.GetTempTestFile("MP3/id3v2.4_UTF8.mp3");
            Track theTrack = new Track(testFileLocation);

            // Update Front picture
            PictureInfo newPicture = new PictureInfo(Commons.ImageFormat.Jpeg, PictureInfo.PIC_TYPE.Front);
            newPicture.PictureData = File.ReadAllBytes(TestUtils.GetResourceLocationRoot() + "_Images/pic2.jpg");
            theTrack.EmbeddedPictures.Add(newPicture);

            theTrack.Save();

            theTrack = new Track(testFileLocation);

            Assert.AreEqual(2, theTrack.EmbeddedPictures.Count); // Front Cover, Conductor

            bool foundFront = false;
            bool foundConductor = false;

            foreach (PictureInfo pic in theTrack.EmbeddedPictures)
            {
                if (pic.PicType.Equals(PictureInfo.PIC_TYPE.Front))
                {
                    foundFront = true;
                    Image picture = Image.FromStream(new MemoryStream(pic.PictureData));
                    Assert.AreEqual(picture.RawFormat, System.Drawing.Imaging.ImageFormat.Jpeg);
                    Assert.AreEqual(picture.Width, 900);
                    Assert.AreEqual(picture.Height, 290);
                }
                if (pic.PicType.Equals(PictureInfo.PIC_TYPE.Unsupported)) foundConductor = true;
            }

            Assert.IsTrue(foundFront);
            Assert.IsTrue(foundConductor);

            // Get rid of the working copy
            File.Delete(testFileLocation);
        }

        [TestMethod]
        public void TagIO_RW_UpdateKeepDataIntegrity()
        {
            Settings.EnablePadding = true;

            try
            {
                string resource = "OGG/ogg.ogg";
                string location = TestUtils.GetResourceLocationRoot() + resource;
                string testFileLocation = TestUtils.GetTempTestFile(resource);
                Track theTrack = new Track(testFileLocation);

                string initialArtist = theTrack.Artist;
                theTrack.Artist = "Hey ho";
                theTrack.Save();

                theTrack = new Track(testFileLocation);

                theTrack.Artist = initialArtist;
                theTrack.Save();

                // Check that the resulting file (working copy that has been processed) remains identical to the original file (i.e. no byte lost nor added)
                FileInfo originalFileInfo = new FileInfo(location);
                FileInfo testFileInfo = new FileInfo(testFileLocation);

                Assert.AreEqual(originalFileInfo.Length, testFileInfo.Length);
                /* Not possible due to field order being changed
                                string originalMD5 = TestUtils.GetFileMD5Hash(location);
                                string testMD5 = TestUtils.GetFileMD5Hash(testFileLocation);

                                Assert.IsTrue(originalMD5.Equals(testMD5));
                */
                // Get rid of the working copy
                File.Delete(testFileLocation);
            }
            finally
            {
                Settings.EnablePadding = false;
            }
        }

        [TestMethod]
        public void TagIO_RW_UpdateKeepTrackDiscZeroes_APE()
        {
            bool settingsInit1 = Settings.UseLeadingZeroes;
            Settings.UseLeadingZeroes = false;
            bool settingsInit2 = Settings.OverrideExistingLeadingZeroesFormat;
            Settings.OverrideExistingLeadingZeroesFormat = false;

            try
            {
                string fileName = "MP3/APE.mp3";
                string location = TestUtils.GetResourceLocationRoot() + fileName;
                string testFileLocation = TestUtils.GetTempTestFile(fileName);
                Track theTrack = new Track(testFileLocation);

                // Update Track count
                theTrack.TrackNumber = 6;
                theTrack.TrackTotal = 6;

                theTrack.Save();

                // TODO - check if formatting of track and disc are correct on-file
                theTrack = new Track(testFileLocation);
                Assert.AreEqual(6, theTrack.TrackNumber);
                Assert.AreEqual(6, theTrack.TrackTotal);
                Assert.AreEqual(3, theTrack.DiscNumber);
                Assert.AreEqual(4, theTrack.DiscTotal);

                // File length should stay the same (which means all leading zeroes are accounted for)
                FileInfo originalFileInfo = new FileInfo(location);
                FileInfo testFileInfo = new FileInfo(testFileLocation);
                Assert.AreEqual(originalFileInfo.Length, testFileInfo.Length);

                // Get rid of the working copy
                File.Delete(testFileLocation);
            }
            finally
            {
                Settings.UseLeadingZeroes = settingsInit1;
                Settings.OverrideExistingLeadingZeroesFormat = settingsInit2;
            }
        }

        [TestMethod]
        public void TagIO_RW_UpdateKeepTrackDiscZeroes_ID3v2()
        {
            string fileName = "MP3/id3v2.4_UTF8.mp3";
            string location = TestUtils.GetResourceLocationRoot() + fileName;
            string testFileLocation = TestUtils.GetTempTestFile(fileName);
            Track theTrack = new Track(testFileLocation);

            // Update Track count
            theTrack.TrackNumber = 6;
            theTrack.TrackTotal = 6;

            theTrack.Save();

            // TODO - check if formatting of track and disc are correct on-file
            theTrack = new Track(testFileLocation);
            Assert.AreEqual(6, theTrack.TrackNumber);
            Assert.AreEqual(6, theTrack.TrackTotal);
            Assert.AreEqual(3, theTrack.DiscNumber);
            Assert.AreEqual(4, theTrack.DiscTotal);

            // File length should stay the same (which means all leading zeroes are accounted for)
            FileInfo originalFileInfo = new FileInfo(location);
            FileInfo testFileInfo = new FileInfo(testFileLocation);
            Assert.AreEqual(originalFileInfo.Length, testFileInfo.Length);

            // Get rid of the working copy
            File.Delete(testFileLocation);
        }

        [TestMethod]
        public void StreamedIO_R_Audio()
        {
            string resource = "OGG/ogg.ogg";
            string location = TestUtils.GetResourceLocationRoot() + resource;
            string testFileLocation = TestUtils.GetTempTestFile(resource);

            using (FileStream fs = new FileStream(testFileLocation, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Track theTrack = new Track(fs, "audio/ogg");

                Assert.AreEqual(33, theTrack.Duration);
                Assert.AreEqual(69, theTrack.Bitrate);
                Assert.AreEqual(22050, theTrack.SampleRate);
                Assert.AreEqual(true, theTrack.IsVBR);
                Assert.AreEqual(AudioDataIOFactory.CF_LOSSY, theTrack.CodecFamily);
            }

            // Get rid of the working copy
            File.Delete(testFileLocation);
        }


        [TestMethod]
        public void StreamedIO_R_Meta()
        {
            string resource = "OGG/ogg.ogg";
            string location = TestUtils.GetResourceLocationRoot() + resource;
            string testFileLocation = TestUtils.GetTempTestFile(resource);

            using (FileStream fs = new FileStream(testFileLocation, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Vorbis_OGG offTest = new Vorbis_OGG();
                offTest.TagIO_R_VorbisOGG_simple_OnePager(fs);
                fs.Seek(0, SeekOrigin.Begin); // Test if stream is still open
            }

            // Get rid of the working copy
            File.Delete(testFileLocation);
        }

        [TestMethod]
        public void StreamedIO_RW_Meta()
        {
            string resource = "OGG/empty.ogg";
            string location = TestUtils.GetResourceLocationRoot() + resource;
            string testFileLocation = TestUtils.GetTempTestFile(resource);

            using (FileStream fs = new FileStream(testFileLocation, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
            {
                Vorbis_OGG offTest = new Vorbis_OGG();
                offTest.TagIO_RW_VorbisOGG_Empty(fs);
                fs.Seek(0, SeekOrigin.Begin); // Test if stream is still open
            }

            // Get rid of the working copy
            File.Delete(testFileLocation);
        }

    }
}
