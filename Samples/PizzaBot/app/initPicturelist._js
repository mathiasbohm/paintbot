(function () {
    "use strict";
    var oDOMParser = new DOMParser(),
        ids = {},
        lastId = 0,
        //cmdURL =  'http://s-fcbg-pus5.cornelsen.de/bot/api/paintcommands',
        cmdURL = '../api/paintcommands',
        CREATE = 0xFF,
        DELETE = 0,
        MOVE = 1,
        SIZE = 2,
        COLOR = 4,
        TRANSFORM = 8;
    function loadData(path, type, callback, error) {
        var oReq = new XMLHttpRequest(), promise = window.newPromise();
        function onload(evt) {
            if (callback) {
                callback(oReq.response);
            }
            promise.resolve(oReq.response);
        }
        function onerror(evt) {
            if (error) {
                error(oReq.responseText);
            }
            promise.reject(oReq.responseText);
        }
        oReq.addEventListener('load', onload);
        oReq.addEventListener('error', error);
        oReq.responseType = type || '';
        oReq.open("GET", path);
        oReq.send();
        return promise;
    }

    function loadSVG(path, callback) {
        var promise = window.newPromise(), svg;//verkettete Promises kann das tinypromise leider nicht
        loadData(path, 'text').then(function (response) {
            svg = oDOMParser.parseFromString(response.responseText, 'text/xml');
        }, function (resText) {
            promise.resolve(resText);
        });
        return promise;
    }

    function alert(msg) {
        var alertp = window.document.getElementById('alert');
        alertp.innerHTML =  msg;
        alertp.style.opacity = 1;
        window.setTimeout(function() {
            alertp.style.opacity = 0;
        }, 5000);
    }

    function newId(main) {
        var newId = main, nr = 0;
        while (ids.hasOwnProperty(newId)) {
            nr += 1;
            newId = main + nr;
        }
        return newId;
    }
    function addPicture(attr) {
        var path = 'assets/'+attr.name + '.svg',
            img = '<image xlink:href="' + path + '"',
            image = oDOMParser.parseFromString(img, 'text/html');
        img += 'class="' + attr.id + '" '
        img +=  'x="' + attr.x + '" y="' + attr.y + '" ';
        if (attr.scale) {
            img += 'width="' + (attr.scale.x || 1) * 100 + '" height="' + (attr.scale.y || 1) * 100 + '"';
        } else {
            img += 'width="100" height="100"';
        }
        img += '></image>';
        //image = image.
        image = document.createElementNS('http://www.w3.org/2000/svg','image');
        image.innerHTML =img;
        /*image['xlink:href'] = path;
        image.width = 50;
        image.height = 50;
        image.x = 30;
        image.y = 30;*/
        paintbot.svg.appendChild(image.firstChild);
    }

    function delElements(elements) {
        var i = 0, len = elements.length;
        if (elements.length !== undefined) {//Eine HTMLCollection
            for (; i < len; i += 1) {
                elements[i].remove();
            }
        } else {//das Element direkt
            elements.remove();
        }
    }

    function makeCmd(cmd) {
        if (cmd.action < 0) {
            cmd.action = MOVE + TRANSFORM + SIZE + COLOR;//Alles was wir können, die entsprechenden Methoden dürfen selber prüfen ob sich was geändert hat.
        }
        switch (cmd.action) {
            case CREATE:
                addPicture(cmd);
                break;
            case DELETE:
                //welches?
                delElements(cmd.elem);
                break;
            default:
                console.log('makeCmd ' + cmd.action);
                break;
        }
    }

    function parseCmd(cmd) {
        var jsCmd = {}, node, i, len, elem;
        jsCmd.id = cmd.identifier;
        elem = window.paintbot.svg.getElementsByClassName(jsCmd.id);
        if (cmd.tagname === undefined || cmd.tagname === null || cmd.tagname === '') {
            if (elem && elem.length > 0) {
                jsCmd.action = DELETE;
                jsCmd.element = elem;
                makeCmd(jsCmd);
            }
            return;
        }
        jsCmd.name = cmd.tagname;
        jsCmd.x = cmd.position.item1 * 1000;
        jsCmd.y = cmd.position.item2 * 800;
        jsCmd.color = cmd.color;
        jsCmd.scale = {
            x: cmd.scale.item1,
            y: cmd.scale.item2
        };
        //jsCmd.width = 100 * cmd.scale.item1;
        //jsCmd.height = 100 * cmd.scale.item2;
        if (elem && elem.length > 0) {
            jsCmd.action = 1;
            jsCmd.element = elem;
            makeCmd(jsCmd);
        } else {
            jsCmd.action = CREATE;
            makeCmd(jsCmd);
            for (i = 1; i < cmd.amount; i += 1) {
                jsCmd.x += 50;
                //jsCmd.y += 0;
                makeCmd(jsCmd);
            }
        }


    }

    function parseCmdlist(commands) {
        var i, len, cmd;
        for (i = 0, len = commands.children.length; i < len; i += 1) {
            cmd = commands.children[i];
            parseCmd(cmd);
            lastId = cmd.id;
        }

    }

    function loadCommands() {
        var url = cmdURL + '?id='+lastId;
        loadData(url,'text').then(function (response) {
            //var paintCmds = oDOMParser.parseFromString(response, 'text/xml');
            var paintCmd;
            if (typeof response !== 'string' || response === '') {
                //alert('Keine weiteren Zeichenanweisungen vorhanden.')
            	return window.setTimeout(loadCommands, 2000);
            }
            paintCmd = JSON.parse(response);
            if (paintCmd === '') {
                //alert('Keine weiteren Zeichenanweisungen vorhanden.')
            	return window.setTimeout(loadCommands, 2000);
            }
            if (typeof paintCmd === 'string') {
                paintCmd = JSON.parse(paintCmd);
            }
            /*if (paintCmds.documentElement.nodeName === 'parseerror') {
                alert('Fehler beim parsen der Commandlist');
                lastId += 1;
                //lastId auf 0 und Bild leeren?
            } else if (paintCmds.documentElement.nodeName === 'html') {
                alert('Paintbotaufruf liefert Fehlerseite');
                return;
            } else {*/
                //TODO: Kommando verarbeiten
                /*var cmd = {
                    "identifier": "d83a9600-bb48-409f-b2a6-c1f679f58d7c",
                    "tagname": "baum",
                    "amount": 2,
                    "metaInfos": [ "bäume" ],
                    "position": { "item1": 0.5, "item2": 0.5, "item3": 0.0 },
                    "scale": { "item1": 1.0, "item2": 1.0 },
                    "color": "#FFFFFF"
                };*/
            if (paintCmd) {
                parseCmd(paintCmd);
            }
            lastId += 1;//paintCmds.documentElement.children.length;
            window.setTimeout(loadCommands, 2000);
        }, function (response) {
           /*var cmd = {
               "identifier": "d83a9600-bb48-409f-b2a6-c1f679f58d7c",
               "tagname": "baum",
               "amount": 2,
               "metaInfos": [ "bäume" ],
               "position": { "item1": 0.5, "item2": 0.5, "item3": 0.0 },
               "scale": { "item1": 1.0, "item2": 1.0 },
               "color": "#FFFFFF"
           };
            parseCmd(cmd);*/
            alert('Paintbot nicht erreichbar:' + response);
        });
        //lastId += 1;
        /*makeCmd({
            name: window.paintbot.pictures[lastId % window.paintbot.pictures.length],
            action: 'create',
            height: 60,
            width: 60,
            x: 20 * lastId,
            y: 20 * lastId
        });*/

    }
    function initPictures() {
        window.paintbot.svg = window.document.getElementById('paintbotview');
        loadData('assets/objects.txt', 'text').then(function (response) {
            window.paintbot.pictures = JSON.parse(response);
            addPicture({'name': paintbot.pictures[7].slice(0, paintbot.pictures[7].length-4), x: 50, y:60, width:100, height:80});
            addPicture({'name': paintbot.pictures[9].slice(0, paintbot.pictures[9].length-4), x: 850, y:760, width:100, height:80});
            //loadCommands();
            window.setTimeout(loadCommands, 2000);
        }, function (response) {
            alert(response);
        });

    }

    window.paintbot = window.paintbot || {};

    //Polyfill für Element.remove (aus MDN)
    if (!('remove' in Element.prototype)) {
        Element.prototype.remove = function() {
            if (this.parentNode) {
                this.parentNode.removeChild(this);
            }
        };
    }
    
    initPictures();

}());