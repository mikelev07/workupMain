var finalFileBuffer = [];
var lastIndexFile = 0;
$(function () {
 

    $('#chatroom').scrollTop($('#chatroom').prop('scrollHeight'));

    /*
    $('#autocomplete-input').keyup(function (e) {
        clearTimeout($.data(this, 'timer'));
        if (e.keyCode == 13)
            search(true);
        else
            $(this).data('timer', setTimeout(search, 5));
    }); */

    function tog(v) { return v ? 'addClass' : 'removeClass'; }
    $(document).on('input', '.clearable', function () {
      
        $(this)[tog(this.value)]('x');
    }).on('mousemove', '.x', function (e) {
        $(this)[tog(this.offsetWidth - 42 < e.clientX - this.getBoundingClientRect().left)]('onX');
        }).on('touchstart click', '.onX', function (ev) {
            $.ajax({
                url: '/Chat/CustomSearch',
                type: 'POST',
                data: { dName: '' },
                dataType: "html",
                success: function (data) {
                    $('#chatusers').html(data);
                }
            });
        ev.preventDefault();
        $(this).removeClass('x onX').val('').change();
    });


    $('#autocomplete-input').keyup(function () {
        var dialogName = $('#autocomplete-input').val();
       

        $.ajax({
            url: '/Chat/CustomSearch',
            type: 'POST',
            data: { dName: dialogName },
            dataType: "html", 
            success: function (data) {

                if (data == "''") {
                    $('#chatusers').html('<li style="margin-top:20px;margin-left:90px">Не найдено диалога</li>');
                    if (dialogName === "")
                        $('#chatusers').html(data)
                } else {

                    $('#chatusers').html(data)
                    if (data.length == 2)
                        $('#chatusers').html('<li style="margin-top:20px;margin-left:90px">Не найдено диалога</li>');
                }
                
              
                  
            }
        });

        /*
        
        $.ajax({
            url: '/Chat/CustomSearch',
            type: 'POST',
            dataType: "html",
            data: dName,
            success: function (data) {
                //$('#ProductsDiv').html(data);
            }
        }); */
    });  

    function search(force) {
        var existingString = $("#autocomplete-input").val();

        if (!force && existingString.length < 3) return; 
       
        $.ajax({
            url: '/Chat/DialogSearchAsync',
            type: 'GET',
            cache: false,
            data: { dName: existingString }
        }).done(function (result) {
           // var dialogHtml = '<li id="-conid" class="" onclick="setValue(this.id)" ><a id="" href="#" onclick="loadHistory(this.id)"><div id="" class="message-avatar"><i id="status" class="status-icon status-offline"></i><img src="../Content/Custom/images/user-avatar-small-03.jpg" alt="" /></div><div class="message-by"><div class="message-by-headline"><h5></h5><span class="notificationNew" id="">4 часа назад</span></div><p id="-lastmess">Новых сообщений<span style="color:white" id="notif" class="nav-tag-mess"></span></p></div></a></li >';
          // alert(1)
           // $('#chatusers').append(dialogHtml);
        });
    }

    var page = 2;

    var isLoading = false;
    $(function () {
        $("#upload_link").on('click', function (e) {
            e.preventDefault();
            $("#uploadAttache:hidden").trigger('click');
        });
    });


    $('#uploadAttache').change(function () {
        var file;
        var fileName;
        var filesArray = $('#uploadAttache')[0].files;
        var fileBuffer = [];
        Array.prototype.push.apply(fileBuffer, filesArray);

        if (finalFileBuffer.length <= 0) {
            for (var i = 0; i < filesArray.length; i++) {
                file = $('#uploadAttache')[0].files[i].name;
                if (file.length > 3)
                    fileName = file.substring(0, 3) + '.' + file.substring(file.lastIndexOf('.') + 1)
                $('#fileUp').append('<span id="' + i + '" class="keyword"><span class="keyword-remove"></span><span class="keyword-text">' + fileName + '</span></span>');
            }
            finalFileBuffer = fileBuffer;
            lastIndexFile = fileBuffer.length //2
        } else {
            finalFileBuffer = finalFileBuffer.concat(fileBuffer);
            for (var i = lastIndexFile; i < finalFileBuffer.length; i++) {
                file = finalFileBuffer[i].name;
                if (file.length > 3)
                    fileName = file.substring(0, 3) + '.' + file.substring(file.lastIndexOf('.') + 1)
                $('#fileUp').append('<span id="' + i + '" class="keyword"><span class="keyword-remove"></span><span class="keyword-text">' + fileName + '</span></span>');
            }
            lastIndexFile = finalFileBuffer.length; //4
        }

   
        $('#fileUp').show();
        $('.message-reply').css('margin-top', "5px");
    });


/*
    function loadNewPage() {
        var temp = $("#chatroom").height(); // определяем высоту документа
        page++;
        $.ajax({
            type: 'GET',
            url: '/Chat/Index/' + page,
            success: function (data) {
                $("#chatroom").prepend(data); // добавляем дату
                $("#chatroom").scrollTop($(document).height() - temp);
            }
        });

        isLoading = false;
    } */

    //$("#chatroom").scroll(function () {
      //  if ($("#chatroom").scrollTop() == 0 && !isLoading) {
       //     isLoading = true;
        //    setTimeout(loadNewPage, 500);
       // }
   // });

            // scrolltop() если равен нулю (то есть если скролл наверху ->)
            // !isLoading (если fasle = true)
            // изначально он false, но при прокрутке isLoading = true
            // setTimeout(loadNewPage, 500) устанавливаем таймаут и загружаем по аяксу

        // Ссылка на автоматически-сгенерированный прокси хаба
    var chat = $.connection.chatHub;
    
    chat.client.displayMessage = function (message, partnerId) {
      
        $('#notification-' + partnerId).html(message);

    };

    var lastMessage;
    chat.client.SayWhoIsTyping = function (html, toUserName, username) {
        var toUserName = '#' + username + '-lastmess';
        $.ajax({
            url: '/Chat/GetLastMessage',
            type: 'GET',
            cache: false,
            data: { userName: username }
        }).done(function (result) {
            lastMessage = result
        });

        lastMessage = $(toUserName).text()
        
        $(toUserName).html('<div>' + htmlEncode(html) + '</div >');
        setInterval(function () { $(toUserName).html('<div>' + htmlEncode(lastMessage) + '</div >'); }, 9000);
    };

        
        // Объявление функции, которая хаб вызывает при получении сообщений
    chat.client.addDialog = function (name, dName, messDesc, unCount) {

            var dialogHtml = '<li id="' + htmlEncode(dName) + '-conid" class="" onclick="setValue(this.id)" ><a id="' + htmlEncode(dName) + '" href="#" onclick="loadHistory(this.id)"><div id="" class="message-avatar"><i id="' + htmlEncode(dName) + '-status" class="status-icon status-offline"></i><img src="../Content/Custom/images/user-avatar-small-03.jpg" alt="" /></div><div class="message-by"><div class="message-by-headline"><h5>' + htmlEncode(dName) + '</h5><span class="notificationNew" id="">4 часа назад</span></div><p id="' + htmlEncode(dName) + '-lastmess">Новых сообщений<span style="color:white" id="' + htmlEncode(dName) + '-notif" class="nav-tag-mess">' + htmlEncode(unCount) + '</span></p></div></a></li >';

            $('#chatusers').append(dialogHtml);
        
        };


        // Объявление функции, которая хаб вызывает при получении сообщений
    chat.client.addMessage = function (name, message, dateSend, fileUrls) {

        var attaches = '';
        var style = 'style = "border:1px dashed white; background-color:#28b661; margin-top:5px;"';
        var spanStyle = 'style = "color:white"';
        var iStyle = 'style = "color:white"';

        var otherStyle = 'style = "border:1px dashed #666; background-color:#f4f4f4;margin-top:5px;"';
        var otherSpanStyle = '';
        var otherIStyle = 'style = "color:#666"';

        if (htmlEncode(name) == $('#username').val()) {
          
            if (fileUrls != null) {
                attaches = initAttaches(fileUrls, attaches, style, spanStyle, iStyle);
                var messageHtml = '<div class="message-bubble me"><div class="message-bubble-inner"><div class="message-avatar"><span style="font-size:14px">' + String(dateSend) + '</span></div><div class="message-text"><p>' + htmlEncode(message) + '</p>' + attaches + ' </div></div><div class="clearfix"></div></div>';
            }
            else {
                var messageHtml = '<div class="message-bubble me"><div class="message-bubble-inner"><div class="message-avatar"><span style="font-size:14px">' + String(dateSend) + '</span></div><div class="message-text"><p>' + htmlEncode(message) + '</p></div></div><div class="clearfix"></div></div>';
            }
            } else {
            if (fileUrls != null) {
                attaches = initAttaches(fileUrls, attaches, otherStyle, otherSpanStyle, otherIStyle);
                var messageHtml = '<div class="message-bubble"><div class="message-bubble-inner"><div class="message-avatar"><span style="font-size:14px">' + String(dateSend) + '</span></div><div class="message-text"><p>' + htmlEncode(message) + '</p>' + attaches + '</div></div><div class="clearfix"></div></div>';
            }
            else {
                var messageHtml = '<div class="message-bubble"><div class="message-bubble-inner"><div class="message-avatar"><span style="font-size:14px">' + String(dateSend) + '</span></div><div class="message-text"><p>' + htmlEncode(message) + '</p></div></div><div class="clearfix"></div></div>';
            }
            }

        if ($('#username').val() != htmlEncode(name)) {
            if ($('#toUserName').val() != htmlEncode(name)) {
                var divVote = $("#" + htmlEncode(name) + "-lastmess");
                var note = '<span style="color:white" id="' + htmlEncode(name) + '-notif" class="nav-tag-mess">0</span>';
               // if (divVote.text().indexOf("Новых сообщений") != 0) {
                //    divVote.html("Новых сообщений" + note);
               // }
                var votes = $("#" + htmlEncode(name) + "-notif");
                var num = $("#" + htmlEncode(name) + "-notif").text();
                votes.text(parseInt(num)+1);
                  
            } else {
                    $('#chatroom').append(messageHtml);
                }
            }
        else
        {
             $('#chatroom').append(messageHtml);
        }
          

        // Добавление сообщений на веб-страницу 
       // $('#chatroom').append(messageHtml);
      
        $('#chatroom').scrollTop($('#chatroom').prop('scrollHeight'));
    };

        // Функция, вызываемая при подключении нового пользователя
        chat.client.onConnected = function (id, userName, allUsers) {

                $('#chatBody').show();
                // установка в скрытых полях имени и id текущего пользователя
                $('#hdId').val(id);
                $('#username').val(userName);
           

                // Добавление всех пользователей
                for (i = 0; i < allUsers.length; i++) {

                    AddUser(allUsers[i].ConnectionId, allUsers[i].UserName);

                }
        }

        // Добавляем нового пользователя
        chat.client.onNewUserConnected = function (id, name) {

            AddUser(id, name);
            if ($('#partnerId').val() != '') {

            }
        }

        // Удаляем пользователя
        chat.client.onUserDisconnected = function (id, userName) {

            //   $('#' + id).remove();
            var htmlName = '#' + userName + '-status';
            var conidName = userName + '-conid';
            $(htmlName).attr('class', 'status-icon status-offline');
           // $(conidName).attr('class', 'status-icon status-offline');
            $('.' + id).remove();
            $('#' + id).attr('id', conidName);
          //  $('#partnerId').val() = id;

            var htmlUserName = '#' + userName + '-st';
            $(htmlUserName).text('не в сети');
            $(htmlUserName).css('color', 'red');
    }


 
    
        // Открываем соединение
        $.connection.hub.start().done(function () {
            chat.server.connect();
          
            $('#sendmessage').on('click', function (e) {
                e.preventDefault();
                var files = document.getElementById('uploadAttache').files;
                if (files.length > 0) {
                    if (window.FormData !== undefined) {
                        var data = new FormData();
                        if (finalFileBuffer.length > 0) {
                            for (var x = 0; x < finalFileBuffer.length; x++) {
                                data.append("file" + x, finalFileBuffer[x]);
                            }
                        } else {
                            for (var x = 0; x < files.length; x++) {
                                data.append("file" + x, files[x]);
                            }
                        }
                      
                        $.ajax({
                            xhr: function () {
                                var xhr = new window.XMLHttpRequest();
                                xhr.upload.addEventListener("progress", function (evt) {
                                    if (evt.lengthComputable) {
                                        var percentComplete = evt.loaded / evt.total;
                                        console.log(percentComplete);
                                        $('.progress').css({
                                            width: percentComplete * 100 + '%'
                                        });
                                        if (percentComplete === 1) {
                                            $('.progress').addClass('hide');
                                        }
                                    }
                                }, false);
                                xhr.addEventListener("progress", function (evt) {
                                    if (evt.lengthComputable) {
                                        var percentComplete = evt.loaded / evt.total;
                                        
                                        $('.progress').css({
                                            width: percentComplete * 100 + '%'
                                        });
                                    }
                                }, false);
                                return xhr;
                            },
                            type: "POST",
                            url: '/Chat/UploadAttach',
                            contentType: false,
                            processData: false,
                            data: data,
                            success: function (result) {
                                chat.server.send($('#username').val(), $('textarea#message').val(), $('#partnerId').val(), $('#toUserName').val(), result);
                                $('textarea#message').val('');
                                $('#fileUp').hide();
                                $('.message-reply').css('margin-top', "15px");
                                document.getElementById("uploadAttache").value = "";
                            },
                            error: function (xhr, status, p3) {
                                alert(xhr.responseText);
                            }
                        });
                    } else {
                        alert("Браузер не поддерживает загрузку файлов HTML5!");
                    }
                }
                else {
                    chat.server.send($('#username').val(), $('textarea#message').val(), $('#partnerId').val(), $('#toUserName').val(), null);
                    $('textarea#message').val('');
                }
            });  


            function delay(callback, ms) {
                var timer = 0;
                return function () {
                    var context = this, args = arguments;
                    clearTimeout(timer);
                    timer = setTimeout(function () {
                        callback.apply(context, args);
                    }, ms || 0);
                };
            }

            $('#message').keydown(delay(function (e) {
                if (e.keyCode != 8) {
                    var encodedName = $('<div />').text($('#username').val() + " печатает...").html();

                    chat.server.isTyping(encodedName, $('#toUserName').val(), $('#username').val());
                   
                }
            },500));

            $('#message').keypress(function (e) {
                var key = e.which;
                if (key == 13)  // the enter key code
                {
                    e.preventDefault()
                    var files = document.getElementById('uploadAttache').files;
                    if (files.length > 0) {
                        if (window.FormData !== undefined) {
                            var data = new FormData();
                            if (finalFileBuffer.length > 0) {
                                for (var x = 0; x < finalFileBuffer.length; x++) {
                                    data.append("file" + x, finalFileBuffer[x]);
                                }
                            } else {
                                for (var x = 0; x < files.length; x++) {
                                    data.append("file" + x, files[x]);
                                }
                            }
                            $.ajax({
                                xhr: function () {
                                    var xhr = new window.XMLHttpRequest();
                                    xhr.upload.addEventListener("progress", function (evt) {
                                        if (evt.lengthComputable) {
                                            var percentComplete = evt.loaded / evt.total;
                                            console.log(percentComplete);
                                            $('.progress').css({
                                                width: percentComplete * 100 + '%'
                                            });
                                            if (percentComplete === 1) {
                                                $('.progress').addClass('hide');
                                            }
                                        }
                                    }, false);
                                    xhr.addEventListener("progress", function (evt) {
                                        if (evt.lengthComputable) {
                                            var percentComplete = evt.loaded / evt.total;

                                            $('.progress').css({
                                                width: percentComplete * 100 + '%'
                                            });
                                        }
                                    }, false);
                                    return xhr;
                                },
                                type: "POST",
                                url: '/Chat/UploadAttach',
                                contentType: false,
                                processData: false,
                                data: data,
                                success: function (result) {
                                    alert(result)
                                    chat.server.send($('#username').val(), $('textarea#message').val(), $('#partnerId').val(), $('#toUserName').val(), String(result));
                                    $('textarea#message').val('');
                                    $('#fileUp').hide();
                                    $('.message-reply').css('margin-top', "15px");
                                    document.getElementById("uploadAttache").value = "";
                                },
                                error: function (xhr, status, p3) {
                                    alert(xhr.responseText);
                                }
                            });
                        } else {
                            alert("Браузер не поддерживает загрузку файлов HTML5!");
                        }
                    }
                    else {
                        chat.server.send($('#username').val(), $('textarea#message').val(), $('#partnerId').val(), $('#toUserName').val(), null);
                        $('textarea#message').val('');
                    }
                }
            });
        });
});

function initAttaches(fileUrls, attaches, style, spanStyle, iStyle) {
    for (var i = 0; i < fileUrls.length; i++) {
        var fileUrl = fileUrls[i];
        if (fileUrl != null) {
            var index = fileUrl.lastIndexOf('\\');
            var fileName = fileUrl.slice(index + 1);
            if (fileName.length > 16)
                fileName = fileName.substring(0, 16) + '.' + fileName.substring(fileName.lastIndexOf('.') + 1);
        }
        attaches += '<a id="' + htmlEncode(fileUrl) + '" onclick="loadAttachChat(this.id)"  '+ style +' class="attachment-box ripple-effect "><span '+ spanStyle +'>' + fileName + '</span><i '+ iStyle +'>Скачать</i></a>';
    }
    return attaches;
}

function setValue(id) {
    document.getElementById('partnerId').value = id;
    $('#notification-' + id).html("4 часа назад");
    $("ul#chatusers>li.active-message").removeClass("active-message")
    if (!$('#' + id).hasClass("active-message"))
        $('#' + id).addClass("active-message");
}
/*
function loadHistory(name) {
    document.getElementById('toUserName').value = name;
    $('#dName').text('Диалог с ' + name);
    $("ul#chatusers>li.active-message").removeClass("active-message")
    if (!$('#' + name + '-conid').hasClass("active-message"))
        $('#'+name+'-conid').addClass("active-message");
    $.ajax({
        url: '/Chat/LoadHistory',
        type: 'GET',
        cache: false,
        data: { userName: name }
    }).done(function (result) {
        $('#chatroom').html(result);
        $('#chatroom').scrollTop($('#chatroom').prop('scrollHeight'));
    });
}
*/


// Кодирование тегов
function htmlEncode(value) {
        var encodedValue = $('<div />').text(value).html();
        return encodedValue;
}
//Добавление нового пользователя
function AddUser(id, name) {
    var userIdStr = $('#hdId').val();
    if (userIdStr != id) {
       //<input type="button" onclick="javascript:setValue(' + sId + ');" value="Начать чат" class="' + id + '">')
       //var userHtml = '<li id="' + id + '"onclick="javascript:setValue(' + sId + ',' + stringUserId + ');"><a href="#" ><div class="message-avatar"><i class="status-icon status-online"></i><img src="images/user-avatar-small-03.jpg" alt="" /></div><div class="message-by"><div class="message-by-headline"><h5>' + name + '</h5><span>4 hours ago</span></div><p>Thanks for reaching out. Im quite busy right now on many</p></div></a></li>';
        //$("#chatusers").append(userHtml);
        var htmlName = '#' + name + '-status';
        var conidName = '#' + name + '-conid';
        $(htmlName).attr('class', 'status-icon status-online');
        $(conidName).attr('id', id);
       // $(conidName).on('click',setValue(name,id));
       // "javascript:@(Html.Raw(String.Format("setValue({0},{1})","'" + d.UserTo.UserName + "'","'" + d.UserTo.ConnectionId + "'")))"

        var htmlUserName = '#' + name + '-st';
        //alert("1");
        $(htmlUserName).text('в сети');
        $(htmlUserName).css('color', 'green');
    }

        // $('#chatroom').load('@Url.Action("LoadHistory", "Chat")?id=' + id + '?myId='      
}

