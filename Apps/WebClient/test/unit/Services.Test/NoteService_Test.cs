//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
namespace HealthGateway.WebClient.Test.Services
{
    using Xunit;
    using Moq;
    using DeepEqual.Syntax;
    using HealthGateway.WebClient.Services;
    using HealthGateway.Database.Models;
    using HealthGateway.Database.Wrapper;
    using HealthGateway.Database.Delegates;
    using Microsoft.Extensions.Logging;
    using HealthGateway.Common.Models;
    using System;
    using System.Linq;
    using HealthGateway.WebClient.Models;
    using System.Collections.Generic;
    using HealthGateway.Common.Delegates;
    using HealthGateway.Common.Constants;
    using HealthGateway.Common.ErrorHandling;

    public class NoteServiceTest
    {
        string hdid = "1234567890123456789012345678901234567890123456789012";

        private Tuple<RequestResult<IEnumerable<UserNote>>, List<UserNote>>  ExecuteGetNotes(string encryptionKey = null, Database.Constants.DBStatusCode notesDBResultStatus = Database.Constants.DBStatusCode.Read)
        {
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey }
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(hdid)).Returns(profileDBResult);

            Mock<ICryptoDelegate> cryptoDelegateMock = new Mock<ICryptoDelegate>();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            List<Note> noteList = new List<Note>();
            noteList.Add(new Note
            {
                HdId = hdid,
                Title = "First Note",
                Text = "First Note text",
                CreatedDateTime = new DateTime(2020, 1, 1)
            });

            noteList.Add(new Note
            {
                HdId = hdid,
                Title = "Second Note",
                Text = "Second Note text",
                CreatedDateTime = new DateTime(2020, 2, 2)
            });
            List<UserNote> userNoteList = null;
            if (encryptionKey != null)
            {
                userNoteList = UserNote.CreateListFromDbModel(noteList, cryptoDelegateMock.Object, encryptionKey).ToList();
            }

            DBResult<IEnumerable<Note>> notesDBResult = new DBResult<IEnumerable<Note>>
            {
                Payload = noteList,
                Status = notesDBResultStatus
            };

            Mock<INoteDelegate> noteDelegateMock = new Mock<INoteDelegate>();
            noteDelegateMock.Setup(s => s.GetNotes(hdid, 0, 500)).Returns(notesDBResult);

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                noteDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object
            );

            var userNoteResult = service.GetNotes(hdid, 0, 500);

            return new Tuple<RequestResult<IEnumerable<UserNote>>, List<UserNote>>(userNoteResult, userNoteList);
        }

        [Fact]
        public void ShouldGetNotes()
        {
            Tuple<RequestResult<IEnumerable<UserNote>>, List<UserNote>> getNotesResult = ExecuteGetNotes("abc", Database.Constants.DBStatusCode.Read);
            var actualResult = getNotesResult.Item1;
            List<UserNote> userNoteList = getNotesResult.Item2;

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload.IsDeepEqual(userNoteList));
        }

        [Fact]
        public void ShouldGetNotesWithDbError()
        {
            Tuple<RequestResult<IEnumerable<UserNote>>, List<UserNote>> getNotesResult = ExecuteGetNotes("abc", Database.Constants.DBStatusCode.Error);
            var actualResult = getNotesResult.Item1;

            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
            Assert.Equal("testhostServer-CI-DB", actualResult.ResultError.ErrorCode);
        }

        [Fact]
        public void ShouldGetNotesWithProfileKeyNotSetError()
        {
            Tuple<RequestResult<IEnumerable<UserNote>>, List<UserNote>> getNotesResult = ExecuteGetNotes(null);
            var actualResult = getNotesResult.Item1;

            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
            Assert.Equal("Profile Key not set", actualResult.ResultError.ResultMessage);
        }

        private Tuple<RequestResult<UserNote>, UserNote>  ExecuteCreateNote(Database.Constants.DBStatusCode dBStatusCode = Database.Constants.DBStatusCode.Created)
        {
            string encryptionKey = "abc";
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey }
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(hdid)).Returns(profileDBResult);

            Mock<ICryptoDelegate> cryptoDelegateMock = new Mock<ICryptoDelegate>();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            UserNote userNote = new UserNote()
            {
                HdId = hdid,
                Title = "Inserted Note",
                Text = "Inserted Note text",
                CreatedDateTime = new DateTime(2020, 1, 1)
            };
            Note note = userNote.ToDbModel(cryptoDelegateMock.Object, encryptionKey);

            var insertResult = new DBResult<Note>
            {
                Payload = note,
                Status = dBStatusCode
            };

            Mock<INoteDelegate> noteDelegateMock = new Mock<INoteDelegate>();
            noteDelegateMock.Setup(s => s.AddNote(It.Is<Note>(x => x.Text == note.Text), true)).Returns(insertResult);

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                noteDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object
            );

            var actualResult = service.CreateNote(userNote);
            return new Tuple<RequestResult<UserNote>, UserNote>(actualResult, userNote);
        }

        [Fact]
        public void ShouldInsertNote()
        {
            Tuple<RequestResult<UserNote>, UserNote> getNotesResult = ExecuteCreateNote(Database.Constants.DBStatusCode.Created);
            var actualResult = getNotesResult.Item1;
            var userNote = getNotesResult.Item2;

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.Null(actualResult.ResultError);
            Assert.True(actualResult.ResourcePayload.IsDeepEqual(userNote));
        }

        [Fact]
        public void ShouldInsertNoteWithDBError()
        {
            Tuple<RequestResult<UserNote>, UserNote> deleteNotesResult = ExecuteCreateNote(Database.Constants.DBStatusCode.Error);
            var actualResult = deleteNotesResult.Item1;
            var userNote = deleteNotesResult.Item2;

            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database), actualResult.ResultError.ErrorCode);
        }

        private Tuple<RequestResult<UserNote>, UserNote> ExecuteUpdateNote(Database.Constants.DBStatusCode dBStatusCode = Database.Constants.DBStatusCode.Updated)
        {
            string encryptionKey = "abc";
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey }
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(hdid)).Returns(profileDBResult);

            Mock<ICryptoDelegate> cryptoDelegateMock = new Mock<ICryptoDelegate>();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            UserNote userNote = new UserNote()
            {
                HdId = hdid,
                Title = "Updated Note",
                Text = "Updated Note text",
                CreatedDateTime = new DateTime(2020, 1, 1)
            };

            Note note = userNote.ToDbModel(cryptoDelegateMock.Object, encryptionKey);

            DBResult<Note> updateResult = new DBResult<Note>
            {
                Payload = note,
                Status = dBStatusCode
            };

            Mock<INoteDelegate> noteDelegateMock = new Mock<INoteDelegate>();
            noteDelegateMock.Setup(s => s.UpdateNote(It.Is<Note>(x => x.Text == note.Text), true)).Returns(updateResult);

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                noteDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object
            );

            RequestResult<UserNote> actualResult = service.UpdateNote(userNote);
            return new Tuple<RequestResult<UserNote>, UserNote>(actualResult, userNote);
        }

        [Fact]
        public void ShouldUpdateNote()
        {
            Tuple<RequestResult<UserNote>, UserNote> getNotesResult = ExecuteUpdateNote(Database.Constants.DBStatusCode.Updated);
            var actualResult = getNotesResult.Item1;
            var userNote = getNotesResult.Item2;

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.True(actualResult.ResourcePayload.IsDeepEqual(userNote));
        }

        [Fact]
        public void ShouldUpdateNoteWithDBError()
        {
            Tuple<RequestResult<UserNote>, UserNote> getNotesResult = ExecuteUpdateNote(Database.Constants.DBStatusCode.Error);
            var actualResult = getNotesResult.Item1;

            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
            Assert.Equal(ErrorTranslator.ServiceError(ErrorType.CommunicationInternal, ServiceType.Database), actualResult.ResultError.ErrorCode);
        }

        private Tuple<RequestResult<UserNote>, UserNote> ExecuteDeleteNote(Database.Constants.DBStatusCode dBStatusCode = Database.Constants.DBStatusCode.Deleted)
        {
            string encryptionKey = "abc";
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey }
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(hdid)).Returns(profileDBResult);

            Mock<ICryptoDelegate> cryptoDelegateMock = new Mock<ICryptoDelegate>();
            cryptoDelegateMock.Setup(s => s.Encrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text + key);
            cryptoDelegateMock.Setup(s => s.Decrypt(It.IsAny<string>(), It.IsAny<string>())).Returns((string key, string text) => text.Remove(text.Length - key.Length));

            UserNote userNote = new UserNote()
            {
                HdId = hdid,
                Title = "Deleted Note",
                Text = "Deleted Note text",
                CreatedDateTime = new DateTime(2020, 1, 1)
            };
            Note note = userNote.ToDbModel(cryptoDelegateMock.Object, encryptionKey);

            DBResult<Note> deleteResult = new DBResult<Note>
            {
                Payload = note,
                Status = dBStatusCode
            };

            Mock<INoteDelegate> noteDelegateMock = new Mock<INoteDelegate>();
            noteDelegateMock.Setup(s => s.DeleteNote(It.Is<Note>(x => x.Text == note.Text), true)).Returns(deleteResult);

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                noteDelegateMock.Object,
                profileDelegateMock.Object,
                cryptoDelegateMock.Object
            );

            RequestResult<UserNote> actualResult = service.DeleteNote(userNote);
            return new Tuple<RequestResult<UserNote>, UserNote>(actualResult, userNote);
        }

        [Fact]
        public void ShouldDeleteNote()
        {
            Tuple<RequestResult<UserNote>, UserNote> deleteNotesResult = ExecuteDeleteNote(Database.Constants.DBStatusCode.Deleted);
            var actualResult = deleteNotesResult.Item1;
            var userNote = deleteNotesResult.Item2;

            Assert.Equal(Common.Constants.ResultType.Success, actualResult.ResultStatus);
            Assert.Null(actualResult.ResultError);
            Assert.True(actualResult.ResourcePayload.IsDeepEqual(userNote));
        }

        [Fact]
        public void ShouldDeleteNoteWithDBError()
        {
            Tuple<RequestResult<UserNote>, UserNote> getNotesResult = ExecuteDeleteNote(Database.Constants.DBStatusCode.Error);
            var actualResult = getNotesResult.Item1;

            Assert.Equal(Common.Constants.ResultType.Error, actualResult.ResultStatus);
            Assert.NotNull(actualResult.ResultError);
        }

        [Fact]
        public void ShouldBeErrorIfNoKeyAdd()
        {
            string encryptionKey = null;
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey }
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(hdid)).Returns(profileDBResult);

            UserNote userNote = new UserNote()
            {
                HdId = hdid,
                Title = "Deleted Note",
                Text = "Deleted Note text",
                CreatedDateTime = new DateTime(2020, 1, 1)
            };

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                new Mock<INoteDelegate>().Object,
                profileDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object
            );

            RequestResult<UserNote> actualResult = service.CreateNote(userNote);
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        [Fact]
        public void ShouldBeErrorIfNoKeyUpdate()
        {
            string encryptionKey = null;
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey }
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(hdid)).Returns(profileDBResult);

            UserNote userNote = new UserNote()
            {
                HdId = hdid,
                Title = "Deleted Note",
                Text = "Deleted Note text",
                CreatedDateTime = new DateTime(2020, 1, 1)
            };

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                new Mock<INoteDelegate>().Object,
                profileDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object
            );

            RequestResult<UserNote> actualResult = service.UpdateNote(userNote);
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);
        }

        [Fact]
        public void ShouldBeErrorNoKeyDelete()
        {
            string encryptionKey = null;
            DBResult<UserProfile> profileDBResult = new DBResult<UserProfile>
            {
                Payload = new UserProfile() { EncryptionKey = encryptionKey }
            };

            Mock<IUserProfileDelegate> profileDelegateMock = new Mock<IUserProfileDelegate>();
            profileDelegateMock.Setup(s => s.GetUserProfile(hdid)).Returns(profileDBResult);

            UserNote userNote = new UserNote()
            {
                HdId = hdid,
                Title = "Deleted Note",
                Text = "Deleted Note text",
                CreatedDateTime = new DateTime(2020, 1, 1)
            };

            INoteService service = new NoteService(
                new Mock<ILogger<NoteService>>().Object,
                new Mock<INoteDelegate>().Object,
                profileDelegateMock.Object,
                new Mock<ICryptoDelegate>().Object
            );

            RequestResult<UserNote> actualResult = service.DeleteNote(userNote);
            Assert.Equal(ResultType.Error, actualResult.ResultStatus);

        }
    }
}
