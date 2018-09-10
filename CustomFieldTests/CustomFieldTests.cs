using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
namespace CustomFieldManager.Tests
{
    [TestClass()]
    public class CustomFieldManagerTests
    {

        #region - Utility -

        #region - Test Entity Factory -

        private static ICustomFieldManager CreateIO() { return new CustomFieldManager(); }
        private static ICustomFieldManager CreateIO(string context)
        {
            var io = new CustomFieldManager();
            io.Import(context);
            return io;
        }
        private static ICustomFieldManager CreateIO(string context, CustomFieldManager.IDVerifyHandler veri)
        {
            var io = new CustomFieldManager(veri);
            io.Import(context);
            return io;
        }

        #endregion

        #region - Data Structure -

        private class AnswerSetting
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public int CycleTime { get; set; }
        }
        private class AnswerField
        {
            public int Combination { get; set; }
            public int TextLimit { get; set; }
            public int RotationFirst { get; set; }
            public int RotationSecond { get; set; }
        }
        AnswerField CreateField(int comb = Constants.NotExistId, int chtr = Constants.NotExistId, int rf = Constants.NotExistId, int rs = Constants.NotExistId)
        {
            return new AnswerField { Combination = comb, TextLimit = chtr, RotationFirst = rf, RotationSecond = rs };
        }
        #endregion

        #region  - Context -

        private const string _contextFromXQ = @"<?xml version='1.0' encoding='big5' ?>
        <Result>
            <TitleList DefaultID='_13'>
                <Title ID='2354230C-E772-4063-AC55-4280D7F70BCB' Name='測試' CycleTime='59' Value='2,49;49;50,49;50,50' TextLimit='8,2,2,8'/>
                <Title ID='337E204A-B47E-4a78-83B8-0A3D64C9F93C' Name='系統' CycleTime='10' Value='2'/>
            </TitleList>
        </Result>";

        private const string _contextError = @"<?xml version='1.0' encoding='big5' ?>
        <Result>
            <TitleList DefaultID='_87'>
                <Title ID='STR' Name='STR' CycleTime='87' Value='0,87;0;87,0;87,0' TextLimit='87,87,87,87'/>
            </TitleList>
        </Result>";

        #endregion

        private static void PassOnlyIfException(Action expectFail)
        {
            try
            {
                expectFail.Invoke();
            }
            catch (Exception e)
            {
                Assert.IsFalse(e is NotImplementedException);
                return;
            }
            Assert.Fail();
        }
        private static void ChunkedEqual(string lhs, string rhs, int chunkSize)
        {
            //Convenience for Debug
            IEnumerable<string> ToChunk(string source, int size)
            {
                return Enumerable.Range(0, source.Length / size)
                    .Select(i => source.Substring(i * size, size));
            }
            foreach (var item in ToChunk(lhs, chunkSize).Zip(
                ToChunk(rhs, chunkSize), (an, ac) => new { Answer = an, Actual = ac }))
            {
                Assert.AreEqual(item.Answer, item.Actual);
            }
        }

        #endregion

        #region - Test Category Title -

        private const string _methodValidity = "Method Validity";
        private const string _equality = "TestData Equality";
        private const string _modifying = "Modifying";
        private const string _emptyState = "Empty State";
        private const string _testValidity = "Test Validity";

        #endregion

        #region - Method Validity -
        
        [TestMethod(), TestCategory(_methodValidity)]
        public void Import_WithVariousContext_ExitExpectedly()
        {
            string existContext = _contextFromXQ;
            string notExistContext = "<?xml version='1.0' encoding='big5' ?><Wrong></Wrong>";
            CreateIO();
            CreateIO(existContext);
            PassOnlyIfException(() => CreateIO(notExistContext));
        }

        [TestMethod, TestCategory(_methodValidity)]
        public void Combination_SetProperty_ReturnEqually()
        {
            var emptyIO = CreateIO();
            var field = emptyIO.AddSetting().AddField();
            int newCombination = 99;

            field.Combination = newCombination;
            Assert.AreEqual(newCombination, field.Combination);
        }

        [TestMethod, TestCategory(_methodValidity)]
        public void TextLimit_SetProperty_ReturnEquallyWithoutOrder()
        {
            var emptyIO = CreateIO();
            var field = emptyIO.AddSetting().AddField();
            int newCombination = 99, newCharacters = 999;

            Assert.AreEqual(Constants.NotExistId, field.Combination);

            void DoAssign() { field.TextLimit = newCharacters; }
            DoAssign();
            field.Combination = newCombination;
            DoAssign();
        }

        [TestMethod, TestCategory(_methodValidity)]
        public void RotationFirst_SetProperty_ReturnEquallyWithoutOrder()
        {
            var emptyIO = CreateIO();
            var field = emptyIO.AddSetting().AddField();
            int newCombination = 99, newRFirst = 999;

            Assert.AreEqual(Constants.NotExistId, field.Combination);

            void DoAssign() { field.RotationFirst = newRFirst; }
            DoAssign();
            field.Combination = newCombination;
            DoAssign();
        }

        [TestMethod, TestCategory(_methodValidity)]
        public void RotationSecond_SetProperty_ReturnEquallyWithoutOrder()
        {
            var emptyIO = CreateIO();
            var field = emptyIO.AddSetting().AddField();
            int newCombination = 99, newRFirst = 999, newRSecond = 9999;

            Assert.AreEqual(Constants.NotExistId, field.Combination);

            void DoAssign() { field.RotationSecond = newRSecond; }
            DoAssign();
            field.Combination = newCombination;
            DoAssign();
            field.RotationFirst = newRFirst;
            DoAssign();
        }

        #endregion

        #region - Equality -

        [TestMethod(), TestCategory(_equality)]
        public void Export_WithValidContext_XMLEqually()
        {
            var importNothing = CreateIO();
            importNothing.Export();

            var testIO = CreateIO(_contextFromXQ);

            var answerTree = XElement.Parse(_contextFromXQ).ToString();
            var actualTree = XElement.Parse(testIO.Export()).ToString();

            ChunkedEqual(answerTree, actualTree, 10);
        }
        [TestMethod(), TestCategory(_equality)]
        public void GetManager_WithValidContext_PropertyEqually()
        {
            Assert.AreEqual(Constants.NotExistId, CreateIO().DefaultID);
            Assert.AreEqual(13, CreateIO(_contextFromXQ).DefaultID);
        }
        [TestMethod(), TestCategory(_equality)]
        public void PeekSettings_WithValidContext_PropertyEqually()
        {
            var answerID = 13;
            var answerList = new List<AnswerSetting>()
            {
                new AnswerSetting { ID = "2354230C-E772-4063-AC55-4280D7F70BCB", Name = "測試", CycleTime = 59},
                new AnswerSetting { ID = "337E204A-B47E-4a78-83B8-0A3D64C9F93C", Name = "系統", CycleTime = 10}
            };

            var structure = CreateIO(_contextFromXQ);
            Assert.AreEqual(answerID, structure.DefaultID);

            void cmp(ICustomFieldSetting answer, AnswerSetting checking)
            {
                Assert.AreEqual(answer.ID, checking.ID);
                Assert.AreEqual(answer.Name, checking.Name);
                Assert.AreEqual(answer.CycleTime, checking.CycleTime);
            }
            foreach (var e in structure.PeekSettings().Zip(answerList, (s, ans) => new { Setting = s, Answer = ans }))
            {
                cmp(e.Setting, e.Answer);
            }
        }
        [TestMethod(), TestCategory(_equality)]
        public void PeekFields_WithValidContext_PropertyEqually()
        {
            var fieldList = CreateIO(_contextFromXQ).PeekSettings().First().PeekFields();

            var answerField = new List<AnswerField>
            {
                CreateField(2,8), CreateField(49,2,49,50), CreateField(49,2,50), CreateField(50,8)
            };
            foreach (var e in fieldList.Zip(answerField, (f, ans) => new { Field = f, Answer = ans }))
            {
                Assert.AreEqual(e.Answer.Combination, e.Field.Combination);
                Assert.AreEqual(e.Answer.RotationFirst, e.Field.RotationFirst);
                Assert.AreEqual(e.Answer.RotationSecond, e.Field.RotationSecond);
                Assert.AreEqual(e.Answer.TextLimit, e.Field.TextLimit);
            }
        }
        [TestMethod(), TestCategory(_equality)]
        public void PeekFields_WithVerifiedContext_PropertyFiltered()
        {
            var errCode = 87;
            var io = CreateIO(_contextError, i => i != errCode);
            Assert.AreNotEqual(Constants.NotExistId, errCode);
            Assert.AreEqual(Constants.NotExistId, io.DefaultID);
            
            var setting = io.PeekSettings().First();
            Assert.AreEqual(errCode, setting.CycleTime);
            var answerField = new List<AnswerField>
            {
                CreateField(comb:0, chtr:errCode), CreateField(rf:0, chtr:errCode), CreateField(comb:0, chtr:errCode), CreateField(comb:0, chtr:errCode)
            };
            foreach (var e in setting.PeekFields().Zip(answerField, (f, ans) => new { Field = f, Answer = ans }))
            {
                Assert.AreEqual(e.Answer.Combination, e.Field.Combination);
                Assert.AreEqual(e.Answer.RotationFirst, e.Field.RotationFirst);
                Assert.AreEqual(e.Answer.RotationSecond, e.Field.RotationSecond);
                Assert.AreEqual(e.Answer.TextLimit, e.Field.TextLimit);
            }
        }

        #endregion

        #region - Empty -

        [TestMethod, TestCategory(_emptyState)]
        public void GetManager_ByDefault_PropertyDefault()
        {
            var importNothing = CreateIO();
            var metadata = importNothing;
            Assert.AreEqual(Constants.NotExistId, metadata.DefaultID);
            Assert.IsFalse(metadata.PeekSettings().Any());
        }

        [TestMethod, TestCategory(_emptyState)]
        public void AddSetting_ByDefault_PropertyDefault()
        {
            var importNothing = CreateIO();
            var emptySetting = importNothing.AddSetting();

            Assert.IsNull(emptySetting.ID);
            Assert.IsNull(emptySetting.Name);
            Assert.AreEqual(Constants.NotExistId, emptySetting.CycleTime);
            Assert.IsFalse(emptySetting.PeekFields().Any());
        }

        [TestMethod, TestCategory(_emptyState)]
        public void AddField_ByDefault_PropertyDefault()
        {
            var emptyIO = CreateIO();
            var emptyField = emptyIO.AddSetting().AddField();

            var invalidId = Constants.NotExistId;
            Assert.AreEqual(invalidId, emptyField.Combination);
            Assert.AreEqual(invalidId, emptyField.TextLimit);
            Assert.AreEqual(invalidId, emptyField.RotationFirst);
            Assert.AreEqual(invalidId, emptyField.RotationSecond);
        }

        #endregion

        #region - Modifying -

        [TestMethod, TestCategory(_modifying)]
        public void AddSetting_OneTime_ChangeSize()
        {
            var empty = CreateIO();
            var metadata = empty;
            Assert.AreEqual(0, metadata.PeekSettings().Count());
            var adding = metadata.AddSetting();
            Assert.AreEqual(1, metadata.PeekSettings().Count());
        }

        [TestMethod, TestCategory(_modifying)]
        public void RemoveSetting_OneTime_ChangeSize()
        {
            var empty = CreateIO();
            var metadata = empty;

            Assert.AreEqual(0, metadata.PeekSettings().Count());
            PassOnlyIfException(() =>
            {
                metadata.RemoveSetting();
            });

            metadata.AddSetting();
            Assert.AreEqual(1, metadata.PeekSettings().Count());
            metadata.RemoveSetting();
            Assert.AreEqual(0, metadata.PeekSettings().Count());
        }

        [TestMethod, TestCategory(_modifying)]
        public void AddField_OneTime_ChangeSize()
        {
            var empty = CreateIO();
            var setting = empty.AddSetting();
            Assert.AreEqual(0, setting.PeekFields().Count());
            var adding = setting.AddField();
            Assert.AreEqual(1, setting.PeekFields().Count());
        }

        [TestMethod, TestCategory(_modifying)]
        public void RemoveField_OneTime_ChangeSize()
        {

            var empty = CreateIO();
            var setting = empty.AddSetting();

            Assert.AreEqual(0, setting.PeekFields().Count());
            PassOnlyIfException(() =>
            {
                setting.RemoveField();
            });

            setting.AddField();
            Assert.AreEqual(1, setting.PeekFields().Count());
            setting.RemoveField();
            Assert.AreEqual(0, setting.PeekFields().Count());
        }
        
        [TestMethod, TestCategory(_modifying)]
        public void Export_ModifyVerifiedContext_NoChange()
        {
            //Invalid
            var errCode = 87;
            var io = CreateIO(_contextError, i => i != errCode);
            var answer = io.Export();

            var firstField = io.PeekSettings().First().PeekFields().ElementAt(1);

            Assert.AreNotEqual(Constants.NotExistId, errCode);
            Assert.AreEqual(Constants.NotExistId, firstField.Combination);
            firstField.Combination = errCode;

            var actual = io.Export();
            ChunkedEqual(answer, actual, 10);
        }

        #endregion

        #region - Test Tools -

        [TestMethod, TestCategory(_testValidity)]
        public void PassOnlyIfException_DoNothing_AssertCaptured()
        {
            try
            {
                PassOnlyIfException(() => { });
            }
            catch (AssertFailedException e)
            {
                return;
            }
            Assert.Fail();
        }

        #endregion
    }
}