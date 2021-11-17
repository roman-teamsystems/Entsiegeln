/// <reference path="types/MicrosoftMaps/Microsoft.Maps.All.d.ts" />
/// <reference path="../node_modules/@types/jquery/index.d.ts" />
class Project {
    constructor() {
        this.bezeichnung = "Bezeichnung";
        this.strasse = "";
        this.plz = "";
        this.details = "";
        this.bilder = null;
        this.bsv = false;
        this.kub = false;
        this.bpf = false;
        this.pzuB = false;
        this.pentsV = false;
        this.vzuG = false;
        this.div = false;
        this.vbeet = false;
        this.pp = false;
        this.ug = false;
        this.azuX = false;
        this.gwPI = false;
        this.ruF = false;
        this.prio = 0;
        this.status = 0;
        this.userId = null;
        this.ratings = null;
    }
}
class Bild {
}
class Rating {
}
class Preferences {
}
var myStyle = {
    "version": "1.*",
    "settings": {},
    "elements": {
        "airport": {
            "visible": false
        },
        "cemetery": {
            "visible": false
        },
        "continent": {
            "visible": false
        },
        "indigenousPeoplesReserve": {
            "visible": false
        },
        "industrial": {
            "visible": false
        },
        "medical": {
            "visible": false
        },
        "military": {
            "visible": false
        },
        "nautical": {
            "visible": false
        },
        "neighborhood": {
            "visible": false
        },
        "underground": {
            "visible": false
        },
        "vegetation": {
            "fillColor": "#FFE8FFEE"
        },
        "point": {
            "visible": false
        },
        "address": {
            "labelColor": "#FF000000",
            "labelVisible": true
        },
        "business": {
            "labelVisible": false,
            "visible": false
        },
        "railway": {
            "labelVisible": true,
            "visible": true
        },
        "road": {
            "fillColor": "#FFFFFFFF",
            "labelOutlineColor": "#FFFFFFFF",
            "labelVisible": true,
            "overwriteColor": true,
            "strokeColor": "#FFFFFFFF",
            "visible": true
        },
        "water": {
            "fillColor": "#FFD2ECFF"
        }
    },
    "extensions": {
        "myNamespace": {
            "myCustomState": {
                "parent": "road",
                "strokeColor": "#FF1500FF"
            }
        }
    }
};
const iconSize = 28;
var projects;
var userRatings;
var maxUserFavorites;
var leftFavorites;
var map;
var drawingManager;
var infobox;
var tooltip;
var drawingShape;
var searchManager;
var projectLayers = new Array(5);
var borderLayers = new Array(1);
var favoriteLayer;
var favoriteMap;
var mapLoaded = false;
var massnahmen;
(function (massnahmen) {
    massnahmen[massnahmen["bpf"] = 0] = "bpf";
    massnahmen[massnahmen["bsv"] = 1] = "bsv";
    massnahmen[massnahmen["vbeet"] = 2] = "vbeet";
    massnahmen[massnahmen["bau"] = 3] = "bau";
    massnahmen[massnahmen["gruen"] = 4] = "gruen";
})(massnahmen || (massnahmen = {}));
var timeOut = 0;
$(document).ready(function () {
    $("#SaveButton").hide();
    $("#CancelButton").hide();
    document.querySelector('.custom-file-input').addEventListener('change', function (e) {
        var name = document.getElementById("customFileInput").files[0].name;
        var nextSibling = e.target.nextElementSibling;
        nextSibling.innerText = name;
        console.log("custom-file-input event listener change: " + name);
    });
    console.log("Window Object Hostname: " + location.hostname);
    userRatings = new Array();
    if (sessionStorage.Id) {
        GetUserRatings(sessionStorage.Id);
    }
    waitForMap();
});
function GetFavoriteMap() {
    var options = {
        url: "/Ratings/GetFavoriteMap",
        type: "GET",
        dataType: "json",
        beforeSend: function (request) {
            console.log("GetFavoriteMap...");
        },
        success: function (data) {
            if (favoriteMap) {
                favoriteMap.clear();
            }
            else {
                favoriteMap = new Map();
            }
            for (var i = 0; i <= Object.keys(data).length; i++) {
                favoriteMap.set(parseInt(Object.keys(data)[i]), Object.values(data)[i]);
            }
            FillFavoriteLayer();
            favoriteLayer.setVisible(false);
            map.layers.insert(favoriteLayer);
            $("#toggle-favorites").unbind("click");
            $("#toggle-favorites").bind("click", () => toggleFavorites());
            console.log("GetFavoriteMap() finished");
        },
        error: function (xhr) {
            console.log(xhr);
            postError(xhr.status, xhr.statusText + " -> GetFavoriteMap");
        }
    };
    $.ajax(options);
}
function FillFavoriteLayer() {
    console.log("FillFavoriteLayer()");
    if (favoriteLayer) {
        favoriteLayer.clear();
    }
    else {
        favoriteLayer = new Microsoft.Maps.Layer();
    }
    for (var layer of projectLayers) {
        var primitives = layer.getPrimitives();
        for (var primitive of primitives) {
            if ((primitive instanceof Microsoft.Maps.Pushpin) && (favoriteMap.has(primitive.metadata.id))) {
                var loc = primitive.getLocation();
                var pin = new Microsoft.Maps.Pushpin(loc, { icon: getSVG("#svgLeaf", 10 + 5 * favoriteMap.get(primitive.metadata.id)) });
                pin.metadata = primitive.metadata;
                Microsoft.Maps.Events.addHandler(pin, 'click', iPrimitiveClicked);
                Microsoft.Maps.Events.addHandler(pin, 'mouseover', iPrimitiveHovered);
                Microsoft.Maps.Events.addHandler(pin, 'mouseout', closeTooltip);
                favoriteLayer.add(pin);
            }
        }
    }
}
function waitForMap() {
    if (mapLoaded == false) {
        console.log("Warte auf Karte... " + timeOut);
        if (timeOut == 5) {
            alert("Karte lädt nicht!");
            return;
        }
        timeOut++;
        setTimeout(waitForMap, 1000);
    }
    else {
        console.log("waitForMap() Karte geladen");
        LoadSVGs();
        fetch('/Users/CheckPermission', {
            method: 'GET'
        }).then(response => {
            console.log("waitForMap() Permission fetched");
            if (response.ok) {
                $("#toggle-new").bind("click", e => toggleNewProjects(e));
                sessionStorage.setItem("Manager", "yes");
            }
            else {
                sessionStorage.setItem("Manager", "no");
            }
            console.log(response);
        });
    }
}
function toggleFavorites() {
    if ($("#toggle-favorites").hasClass("active")) {
        $("#toggle-favorites").removeClass("active");
        $("#toggle-bpf").addClass("active");
        $("#toggle-bsv").addClass("active");
        $("#toggle-vbeet").addClass("active");
        $("#toggle-bau").addClass("active");
        $("#toggle-gruen").addClass("active");
        favoriteLayer.setVisible(false);
        for (var layer of projectLayers) {
            layer.setVisible(true);
        }
    }
    else {
        $("#toggle-favorites").addClass("active");
        $("#toggle-bpf").removeClass("active");
        $("#toggle-bsv").removeClass("active");
        $("#toggle-vbeet").removeClass("active");
        $("#toggle-bau").removeClass("active");
        $("#toggle-gruen").removeClass("active");
        for (var layer of projectLayers) {
            layer.setVisible(false);
        }
        favoriteLayer.setVisible(true);
    }
}
function toggleNewProjects(e) {
    if (!$("#toggle-bpf").hasClass("active"))
        $("#toggle-bpf").addClass("active");
    if (!$("#toggle-bsv").hasClass("active"))
        $("#toggle-bsv").addClass("active");
    if (!$("#toggle-vbeet").hasClass("active"))
        $("#toggle-vbeet").addClass("active");
    if (!$("#toggle-bau").hasClass("active"))
        $("#toggle-bau").addClass("active");
    if (!$("#toggle-gruen").hasClass("active"))
        $("#toggle-gruen").addClass("active");
    if ($("#toggle-new").hasClass("active")) {
        $("#toggle-new").removeClass("active");
        InitData(false);
    }
    else {
        $("#toggle-new").addClass("active");
        InitData(true);
    }
}
function ShowUserInfo(detailsDialog) {
    if (sessionStorage.Id) {
        leftFavorites = maxUserFavorites - userRatings.filter(r => r.favorite == true).length;
        console.log(leftFavorites + ":" + maxUserFavorites);
        if (detailsDialog) {
            $("#leftFavorites").show();
            $("#leftFavorites").html(leftFavorites.toString());
            $("#userInfo").hide();
        }
        else {
            $("#leftFavorites").hide();
            $("#userInfo").show();
            $("#userInfo_userName").html(sessionStorage.Username);
            $("#userInfo_leftFavorites").html(leftFavorites);
            $("#userInfo_maxFavorites").html(maxUserFavorites);
        }
    }
}
function GetMaxNumberOfFavorites(userId) {
    $.get("/Users/GetMaxNumberOfFavorites/" + userId, function (data) {
        maxUserFavorites = data;
        ShowUserInfo(false);
    });
    //var options = {
    //	url: "/Users/GetMaxNumberOfFavorites/" + userId,
    //	type: "GET",
    //	dataType: "json",
    //	beforeSend: function (request) {
    //	},
    //	success: function (data) {
    //		maxUserFavorites = data;
    //		//console.log("MaxNumberOfFavorites: ", maxUserFavorites);
    //	},
    //	error: function (xhr) {
    //		console.log(xhr);
    //	}
    //}
    //$.ajax(options);
}
function LoadSVGs() {
    fetch('/Preferences/GetImages', {
        method: 'GET'
    }).then(response => {
        response.json().then((data) => {
            $("#svgBaum").html(data.icon1);
            $("#svgBSV").html(data.icon2);
            $("#svgVbeet").html(data.icon3);
            $("#svgBau").html(data.icon4);
            $("#svgGruen").html(data.icon5);
            $("#toggle-bpf").html(data.icon1);
            $("#toggle-bpf").bind("click", e => toggleMassnahme(e, 0));
            $("#toggle-bsv").html(data.icon2);
            $("#toggle-bsv").bind("click", e => toggleMassnahme(e, 1));
            $("#toggle-vbeet").html(data.icon3);
            $("#toggle-vbeet").bind("click", e => toggleMassnahme(e, 2));
            $("#toggle-bau").html(data.icon4);
            $("#toggle-bau").bind("click", e => toggleMassnahme(e, 3));
            $("#toggle-gruen").html(data.icon5);
            $("#toggle-gruen").bind("click", e => toggleMassnahme(e, 4));
            console.log("LoadSVGs() finished");
            InitData(false);
        });
    });
}
function toggleMassnahme(e, id) {
    map.layers[id].setVisible(!(map.layers[id].getVisible()));
    if ($("#toggle-" + massnahmen[id].toString()).hasClass("active")) {
        $("#toggle-" + massnahmen[id].toString()).removeClass("active");
    }
    else {
        $("#toggle-" + massnahmen[id].toString()).addClass("active");
    }
}
function getSVG(name, size) {
    $(name).children().attr("width", size + "px");
    $(name).children().attr("height", size + "px");
    return $(name).html();
}
async function FileUpload(oFormElement) {
    const formData = new FormData(oFormElement);
    $("#customFileInputButton").prop("disabled", true);
    console.log("Going to upload the image...");
    $("#details-dialog-message").text("Bild wird hochgeladen...");
    $("#details-dialog-message").show();
    try {
        console.log(oFormElement.action);
        const response = await fetch(oFormElement.action, {
            method: 'POST',
            body: formData
        });
        if (response.ok) {
            $("#details-dialog-message").text("Bild hochgeladen");
            setTimeout(function () { $("#details-dialog-message").hide(); }, 3000);
            response.json().then(data => { PostNewImage(data.name); });
            //$("#fileLabel").text("Bild wählen");
            console.log("File uploaded correctly");
        }
        else {
            console.log(response);
            postError(response.status, response.statusText + " -> FileUpload");
            $("#details-dialog-message").text("Fehler beim Hochladen");
            setTimeout(function () { $("#details-dialog-message").hide(); }, 3000);
        }
        $("#customFileInputButton").prop("disabled", false);
    }
    catch (error) {
        postError(999, error.message);
        $("#details-dialog-message").text("Fehler beim Hochladen");
        console.log("Fetch Exception beim File Upload");
        setTimeout(function () { $("#details-dialog-message").hide(); }, 3000);
        $("#customFileInputButton").prop("disabled", false);
        console.error('Error:', error);
    }
}
function GetProjectsData(onlynew) {
    var geturl;
    if (sessionStorage.Id) {
        geturl = "/api?bezirk=" + sessionStorage.bezirk + "&userId=" + sessionStorage.Id + "&onlynew=" + onlynew;
    }
    else {
        geturl = "/api?bezirk=" + sessionStorage.bezirk;
    }
    var options = {
        url: geturl,
        type: "GET",
        dataType: "json",
        beforeSend: function (request) {
            console.log("GetProjectsData()...");
        },
        success: function (data) {
            console.log("GetProjectsData() success");
            projects = data;
            for (const element of projects) {
                DrawProject(element);
            }
            if (onlynew) {
                $("#toggle-favorites").prop("disabled", true);
                $("#toggle-favorites").removeClass("active");
            }
            else {
                $("#toggle-favorites").removeClass("active");
                $("#toggle-favorites").prop("disabled", false);
                GetFavoriteMap();
            }
        },
        error: function (xhr) {
            console.log(xhr);
            postError(xhr.status, xhr.statusText + " -> GetProjectsData");
        }
    };
    $.ajax(options);
}
function GetUserRatings(userId) {
    var options = {
        url: "/Ratings/" + userId,
        type: "GET",
        dataType: "json",
        beforeSend: function (request) {
            console.log("GetUserRatings");
            console.log(options.url);
        },
        success: function (data) {
            userRatings = data;
            console.log(userRatings);
            GetMaxNumberOfFavorites(sessionStorage.Id);
        },
        error: function (xhr) {
            console.log(xhr);
            postError(xhr.status, xhr.statusText + " -> GetUserRatings");
        }
    };
    $.ajax(options);
}
function DrawProject(project) {
    var prim;
    //Module loaded in GetMap()
    prim = Microsoft.Maps.WellKnownText.read(project.wktKoordinaten);
    prim.metadata = project;
    addToEntityLayer(prim);
}
function SaveNewProject(primitive) {
    var project = primitive.metadata;
    Microsoft.Maps.loadModule('Microsoft.Maps.WellKnownText', function () {
        project.datum = new Date(Date.now());
        project.bezirk = Number(sessionStorage.bezirk);
        project.wktKoordinaten = Microsoft.Maps.WellKnownText.write(primitive);
        PostNewProject(project, primitive);
    });
}
;
function PostNewProject(project, primitive) {
    var options = {
        url: "/api",
        type: "POST",
        data: JSON.stringify(project),
        contentType: "application/json",
        beforeSend: function (request) {
            console.log("Going to send...");
        },
        success: function (response) {
            console.log("Success");
            primitive.metadata = response;
            addToEntityLayer(primitive);
        },
        error: function (xhr) {
            console.log(xhr);
            postError(xhr.status, xhr.statusText + " -> PostNewProject");
            alert("Achtung!\n\rProjekte können nur von registrierten Usern angelegt werden.\n\rDie eingegeben Daten werden nicht in der Datenbank gespeichert.");
            primitive.metadata.status = -1;
            addToEntityLayer(primitive);
        }
    };
    $.ajax(options);
}
function PostNewImage(guidName) {
    console.log(guidName);
    var bild = new Bild();
    bild.projectId = g_project.id;
    bild.name = guidName;
    var options = {
        url: "/api/Bilder",
        type: "POST",
        data: JSON.stringify(bild),
        contentType: "application/json",
        beforeSend: function (request) {
            console.log("Going to POST JSON to api/Bilder...");
        },
        success: function (response) {
            console.log(response);
            bild = response;
            g_project.bilder.push(bild);
            ShowProjectImages(g_project.bilder);
            console.log("Success");
        },
        error: function (xhr) {
            console.log("Fehler");
            console.log(xhr);
            postError(xhr.status, xhr.statusText + " -> PostNewImage");
        }
    };
    $.ajax(options);
}
function PostNewRating(project, rating, callback) {
    var options = {
        url: "/Ratings",
        type: "POST",
        data: JSON.stringify(rating),
        contentType: "application/json",
        beforeSend: function (request) {
            console.log("Going to post Rating...");
            console.log(rating);
        },
        success: function (response) {
            console.log("Rating posted");
            console.log(response);
            userRatings.push(response);
            callback();
        },
        error: function (xhr) {
            console.log(xhr);
            postError(xhr.status, xhr.statusText + " -> PostNewRating");
        }
    };
    $.ajax(options);
}
function PutChangedRating(index, rating, callback) {
    var options = {
        url: "/Ratings/" + rating.id,
        type: "PUT",
        data: JSON.stringify(rating),
        contentType: "application/json",
        beforeSend: function (request) {
            console.log("Going to put Rating...");
            console.log(rating);
        },
        success: function () {
            console.log("Rating changed");
            console.log(rating);
            userRatings[index] = rating;
            callback();
        },
        error: function (xhr) {
            console.log(xhr);
            postError(xhr.status, xhr.statusText + " -> PutChangedRating (error)");
        }
    };
    $.ajax(options);
}
function PutChangedProject(project) {
    var options = {
        url: "/api/" + project.id,
        type: "PUT",
        data: JSON.stringify(project),
        contentType: "application/json",
        beforeSend: function (request) {
            console.log("Going to send...");
        },
        success: function () {
            console.log("Success");
            $("#details-dialog-message").text("Gespeichert");
            $("#details-dialog-message").show();
            setTimeout(function () { $("#details-dialog-message").hide(); }, 3000);
            $("#details-dialog-speichern").text("Speichern");
            $("#details-dialog-speichern").prop("disabled", false);
        },
        error: function (xhr) {
            console.log(xhr);
            var message = xhr.status + ': (' + xhr.statusText + ')';
            $("#details-dialog-message").text("Fehler " + message);
            $("#details-dialog-message").show();
            setTimeout(function () { $("#details-dialog-message").hide(); }, 3000);
            $("#details-dialog-speichern").text("Speichern");
            $("#details-dialog-speichern").prop("disabled", false);
            postError(xhr.status, xhr.statusText + " -> PutChangedProject (error)");
        }
    };
    $.ajax(options);
}
function InitData(onlynew) {
    if (map.layers.length > 0) {
        map.layers.clear();
    }
    for (var i = 0; i < projectLayers.length; i++) {
        projectLayers[i] = new Microsoft.Maps.Layer();
    }
    borderLayers[0] = new Microsoft.Maps.Layer();
    Microsoft.Maps.loadModule('Microsoft.Maps.WellKnownText', function () {
        GetProjectsData(onlynew);
        map.layers.insertAll(projectLayers);
        map.layers.insert(borderLayers[0]);
        console.log("InitData() finished");
    });
}
function GetMap() {
    var Zentrum;
    var Bereich;
    if (sessionStorage.bezirk == 2) {
        Zentrum = new Microsoft.Maps.Location(52.50627213801224, 13.423410675101962);
        Bereich = new Microsoft.Maps.LocationRect(Zentrum, 0.15, 0.065);
    }
    else if (sessionStorage.bezirk == 7) {
        Zentrum = new Microsoft.Maps.Location(52.452969089343576, 13.383942491743676);
        Bereich = new Microsoft.Maps.LocationRect(Zentrum, 0.3, 0.2);
    }
    else if (sessionStorage.bezirk == 3) {
        Zentrum = new Microsoft.Maps.Location(52.595726123472545, 13.441969928386829);
        Bereich = new Microsoft.Maps.LocationRect(Zentrum, 0.3, 0.2);
    }
    else if (sessionStorage.bezirk == 6) {
        Zentrum = new Microsoft.Maps.Location(52.43596603419558, 13.257029798919826);
        Bereich = new Microsoft.Maps.LocationRect(Zentrum, 0.3, 0.2);
    }
    else if (sessionStorage.bezirk == 11) {
        Zentrum = new Microsoft.Maps.Location(52.52651095361164, 13.496002293974602);
        Bereich = new Microsoft.Maps.LocationRect(Zentrum, 0.3, 0.2);
    }
    map = new Microsoft.Maps.Map('#myMap', {
        credentials: 'AtjRMMexXb8SbkvunWD_M8mXGoHUPgvmoI3PJ7Hcy4n7-I5ZNJ_FJNoPPP3EqKP_',
        customMapStyle: myStyle,
        center: Zentrum,
        zoom: 14,
        maxBounds: Bereich
    });
    Microsoft.Maps.loadModule('Microsoft.Maps.AutoSuggest', function () {
        var manager = new Microsoft.Maps.AutosuggestManager({ map: map });
        manager.attachAutosuggest('#searchBox', '#searchBoxContainer', selectedSuggestion);
    });
    //SearchOverlay.prototype = new Microsoft.Maps.CustomOverlay({ beneathLabels: false });
    //function SearchOverlay() {
    //	this.searchBox = document.createElement('input');
    //	this.searchBox.type = 'button';
    //	this.searchBox.value = 'Test';
    //	this.searchBox.onclick = function () {
    //	};
    //}
    //SearchOverlay.prototype.onAdd = function () {
    //	var container = document.createElement('div');
    //	container.appendChild(this.searchBox);
    //	container.style.position = 'relative';
    //	container.style.top = '100px';
    //	container.style.left = '100px';
    //	this.setHtmlElement(container);
    //};
    //var overlay = new SearchOverlay();
    //map.layers.insert(overlay);
    Microsoft.Maps.loadModule('Microsoft.Maps.DrawingTools', function () {
        //Create an instance of the DrawingTools class and bind it to the map.
        var tools = new Microsoft.Maps.DrawingTools(map);
        //Show the drawing toolbar and enable editting on the map.
        tools.showDrawingManager(function (manager) {
            //Store a reference to the drawing manager as it will be useful later.
            drawingManager = manager;
            var da = Microsoft.Maps.DrawingTools.DrawingBarAction;
            manager.setOptions({ drawingBarActions: da.polyline | da.polygon | da.point });
            //Add events to the drawing manager.
            Microsoft.Maps.Events.addHandler(manager, 'drawingStarted', DrawingStarted);
            Microsoft.Maps.Events.addHandler(manager, 'drawingEnded', DrawingEnded);
        });
    });
    tooltip = new Microsoft.Maps.Infobox(map.getCenter(), {
        visible: false,
        showPointer: false,
        showCloseButton: false,
        offset: new Microsoft.Maps.Point(-75, 10)
    });
    tooltip.setMap(map);
    infobox = new Microsoft.Maps.Infobox(map.getCenter(), {
        visible: false
    });
    infobox.setMap(map);
    Microsoft.Maps.loadModule('Microsoft.Maps.Search', function () {
        searchManager = new Microsoft.Maps.Search.SearchManager(map);
    });
    Microsoft.Maps.Events.addHandler(map, "click", function (e) {
        infobox.setOptions({ visible: false });
        return true;
    });
    mapLoaded = true;
    console.log("mapLoaded=" + mapLoaded);
}
var searchPin = null;
function selectedSuggestion(result) {
    //Remove previously selected suggestions from the map.
    if (searchPin != null) {
        map.entities.remove(searchPin);
        searchPin = null;
    }
    //Show the suggestion as a pushpin and center map over it.
    searchPin = new Microsoft.Maps.Pushpin(result.location);
    map.entities.push(searchPin);
    map.setView({ bounds: result.bestView });
}
function addToEntityLayer(primitive) {
    var massnahmenId = getMassnahmenId(primitive.metadata);
    if (massnahmenId != -1) {
        Microsoft.Maps.Events.addHandler(primitive, 'click', iPrimitiveClicked);
        Microsoft.Maps.Events.addHandler(primitive, 'mouseover', iPrimitiveHovered);
        Microsoft.Maps.Events.addHandler(primitive, 'mouseout', closeTooltip);
        if (primitive instanceof Microsoft.Maps.Polyline) {
            var loc = primitive.getLocations()[0];
            var icon = new Microsoft.Maps.Pushpin(loc, { icon: getSVG(getSvgId(primitive.metadata), iconSize) });
            icon.metadata = primitive.metadata;
            Microsoft.Maps.Events.addHandler(icon, 'click', iPrimitiveClicked);
            Microsoft.Maps.Events.addHandler(icon, 'mouseover', iPrimitiveHovered);
            Microsoft.Maps.Events.addHandler(icon, 'mouseout', closeTooltip);
            projectLayers[massnahmenId].add(icon);
            primitive.setOptions({ strokeThickness: 4, strokeColor: "green" });
            projectLayers[massnahmenId].add(primitive);
        }
        else if (primitive instanceof Microsoft.Maps.Polygon) {
            Microsoft.Maps.loadModule("Microsoft.Maps.SpatialMath", function () {
                var loc = Microsoft.Maps.SpatialMath.Geometry.centroid(primitive);
                var icon = new Microsoft.Maps.Pushpin(loc, { icon: getSVG(getSvgId(primitive.metadata), iconSize) });
                icon.metadata = primitive.metadata;
                Microsoft.Maps.Events.addHandler(icon, 'click', iPrimitiveClicked);
                Microsoft.Maps.Events.addHandler(icon, 'mouseover', iPrimitiveHovered);
                Microsoft.Maps.Events.addHandler(icon, 'mouseout', closeTooltip);
                projectLayers[massnahmenId].add(icon);
            });
            primitive.setOptions({ strokeThickness: 2, strokeColor: "green" });
            projectLayers[massnahmenId].add(primitive);
        }
        else if (primitive instanceof Microsoft.Maps.Pushpin) {
            primitive.setOptions({
                icon: getSVG(getSvgId(primitive.metadata), iconSize)
            });
            projectLayers[massnahmenId].add(primitive);
        }
    }
    else {
        borderLayers[0].add(InvertPolygon(primitive));
    }
}
function InvertPolygon(border) {
    Microsoft.Maps.loadModule('Microsoft.Maps.SpatialMath', function () {
        var rings = new Array(2);
        var temp = map.getBounds();
        temp.buffer(20);
        var exteriorRing = Microsoft.Maps.SpatialMath.locationRectToPolygon(temp);
        var interiorRing = border.getLocations();
        rings[0] = exteriorRing.getLocations();
        rings[1] = interiorRing;
        var tempPoly = Microsoft.Maps.SpatialMath.Geometry.makeValid(new Microsoft.Maps.Polygon(rings));
        var validatedRings = tempPoly.getRings();
        border.setRings(validatedRings);
        border.setOptions({ strokeThickness: 0, fillColor: 'rgba(0, 0, 0, 0.1)' });
    });
    return border;
}
function DrawingStarted() {
    $("#SaveButton").show();
    $("#CancelButton").show();
}
var canceled = false;
function DrawingEnded() {
    var temp = drawingManager.getPrimitives()[0];
    if (temp) {
        drawingShape = temp;
        drawingManager.clear();
        if (searchPin != null) {
            map.entities.remove(searchPin);
            $('#searchBox').val('');
            searchPin = null;
        }
        if (!canceled) {
            ShowDialog();
        }
    }
    $("#SaveButton").hide();
    $("#CancelButton").hide();
    canceled = false;
}
function CancelDrawing() {
    canceled = true;
    $("#SaveButton").hide();
    $("#CancelButton").hide();
    drawingManager.setDrawingMode(Microsoft.Maps.DrawingTools.DrawingMode.none);
}
function SaveGeometry() {
    canceled = false;
    drawingManager.setDrawingMode(Microsoft.Maps.DrawingTools.DrawingMode.none);
    $("#SaveButton").hide();
    $("#CancelButton").hide();
}
//Dialog: Speichern der Koordinaten und Abfrage der Adresse
function ShowDialog() {
    document.getElementById('save').addEventListener("click", CloseDialog); //nach document.loaded
    document.getElementById('cancel').addEventListener("click", CloseDialog);
    $('#SpeichernDialog').on('shown.bs.modal', function (e) {
        $('#Massnahme').trigger('focus');
    });
    var primitiveType = 0;
    if (drawingShape instanceof Microsoft.Maps.Pushpin)
        primitiveType = 1;
    if (drawingShape instanceof Microsoft.Maps.Polyline)
        primitiveType = 2;
    if (drawingShape instanceof Microsoft.Maps.Polygon)
        primitiveType = 3;
    var loc;
    if (primitiveType == 1) {
        loc = drawingShape.getLocation();
    }
    else {
        loc = drawingShape.getLocations()[0];
    }
    var searchRequest = {
        location: loc,
        callback: function (r) {
            $("#ModalMessage").hide();
            $('#Beitragender').val(sessionStorage.Username);
            if (primitiveType == 1)
                $("#Strasse").val(r.address.addressLine);
            else
                $("#Strasse").val(r.address.addressLine);
            $("#Plz").val(r.address.postalCode);
            $('#SpeichernDialog').modal("show");
        },
        errorCallback: function (e) {
            console.log("Unable to reverse geocode location.");
            $("#ModalMessage").show();
            $('#Beitragender').val(sessionStorage.Username);
            $("#Strasse").val("");
            $("#Plz").val("");
            $('#SpeichernDialog').modal("show");
        }
    };
    searchManager.reverseGeocode(searchRequest);
}
function CloseDialog(e) {
    if (e.target.value == "save") {
        if ($("#Massnahme").val() == null) {
            alert("Maßnahme auswählen.");
            return;
        }
        var project = new Project();
        project.bilder = new Array();
        project.bezeichnung = $("#Bezeichnung").val();
        project.beitragender = $("#Beitragender").val();
        project.userId = sessionStorage.Id;
        project.strasse = $("#Strasse").val();
        project.plz = $("#Plz").val();
        if ($("#Massnahme").val() == 1) {
            project.bpf = true;
            project.pzuB = true;
        }
        else if ($("#Massnahme").val() == 2) {
            project.bsv = true;
        }
        else if ($("#Massnahme").val() == 3) {
            project.pp = true;
            project.ug = true;
            project.vbeet = true;
        }
        else if ($("#Massnahme").val() == 4) {
            project.kub = true;
            project.pentsV = true;
            project.azuX = true;
            project.gwPI = true;
        }
        else if ($("#Massnahme").val() == 5) {
            project.vzuG = true;
        }
        drawingShape.metadata = project;
        SaveNewProject(drawingShape);
    }
    $('#SpeichernDialog').modal("hide");
    $("#Bezeichnung").val("");
    $("#Massnahme").val("0");
    $("#Beitragender").val("");
    $("#Strasse").val("");
    $("#Plz").val("");
}
var infoboxTemplate = `
<div class="customInfobox rounded border text-center">
	<div class="title">{title}</div>
	<p>{address}</p>
	<!--<button class="btn btn-outline-primary" id="thumbUp">
		<img src="/thumb-up.svg" style="width:20px" />
	</button>-->
	<button type="button" id="infoButton" class="btn btn-success text-center">Ansehen</button>
	<!--<button class="btn btn-outline-primary" id="thumbDown">
		<img src="/thumb-down.svg" style="width:20px" />-->
	</button>
</div>
`;
function iPrimitiveClicked(e) {
    closeTooltip();
    var project = e.target.metadata;
    var htmlContent = infoboxTemplate
        .replace('{title}', getMassnahme(project))
        .replace('{address}', project.strasse);
    infobox.setOptions({
        htmlContent: htmlContent,
        location: e.location,
        offset: new Microsoft.Maps.Point(-100, 50),
        visible: true
    });
    $("#infoButton").bind("click", function () {
        infobox.setOptions({
            visible: false
        });
        var loc = e.location;
        if (e.target instanceof Microsoft.Maps.Pushpin)
            loc = e.target.getLocation();
        map.setView({
            mapTypeId: Microsoft.Maps.MapTypeId.birdseye,
            center: loc,
            zoom: 20
        });
        OpenDetailsView(e.target);
    });
}
function VoteOn() {
    $("#likeButton").prop("disabled", false);
    $("#favoriteButton").prop("disabled", false);
}
function VoteOff() {
    $("#likeButton").prop("disabled", true);
    $("#favoriteButton").prop("disabled", true);
}
var g_project;
function OpenDetailsView(primitive) {
    ShowUserInfo(true);
    $("#searchBoxContainer").hide();
    $("#MapFilter").css("visibility", "hidden");
    //if (primitive instanceof Microsoft.Maps.Pushpin) {
    //	primitive.setOptions({ icon: getSVG(getSvgId(primitive.metadata), 64) });
    //}
    g_project = primitive.metadata;
    console.log("OpenDetailsView");
    console.log("g_project = ", g_project);
    $("#details-dialog-contributor").html(g_project.beitragender);
    $("#details-dialog-title").html(getMassnahme(g_project));
    $("#details-dialog-street").text(g_project.strasse);
    $("#details-dialog-ort").text(g_project.bezeichnung);
    $("#bsv").prop("checked", g_project.bsv);
    $("#kub").prop("checked", g_project.kub);
    $("#bpf").prop("checked", g_project.bpf);
    $("#pzub").prop("checked", g_project.pzuB);
    $("#pentsv").prop("checked", g_project.pentsV);
    $("#vzug").prop("checked", g_project.vzuG);
    $("#div").prop("checked", g_project.div);
    $("#vbeet").prop("checked", g_project.vbeet);
    $("#pp").prop("checked", g_project.pp);
    $("#ug").prop("checked", g_project.ug);
    $("#azux").prop("checked", g_project.azuX);
    $("#gwpi").prop("checked", g_project.gwPI);
    $("#ruf").prop("checked", g_project.ruF);
    $("#details-dialog-details").val(g_project.details);
    $("#details-dialog").css("visibility", "visible");
    $("#btnLegende").unbind("click");
    $("#btnLegende").bind("click", function () {
        $("#modalLegende").modal("show");
    });
    $("#details-dialog-schliessen").unbind("click");
    $("#details-dialog-schliessen").bind("click", function () {
        $("#searchBoxContainer").show();
        $("#MapFilter").css("visibility", "visible");
        //if (primitive instanceof Microsoft.Maps.Pushpin) {
        //	primitive.setOptions({ icon: getSVG(getSvgId(primitive.metadata), iconSize) });
        //}
        $("#BilderCarousel").empty();
        $("#details-dialog").css("visibility", "hidden");
        console.log("DetailsDialogSchließen");
        console.log(getMassnahme(g_project) + " " + g_project.strasse);
        console.log(getMassnahme(primitive.metadata) + " " + primitive.metadata.strasse);
        var loc;
        if (primitive instanceof Microsoft.Maps.Pushpin)
            loc = primitive.getLocation();
        else
            loc = primitive.getLocations()[0];
        map.setView({
            mapTypeId: Microsoft.Maps.MapTypeId.canvasLight,
            center: loc,
            zoom: 16
        });
        ShowUserInfo(false);
    });
    $('#details-dialog-loeschen').unbind('click');
    $('#details-dialog-loeschen').bind("click", function () {
        if (confirm("Vorschlag löschen?")) {
            console.log("Ja");
            fetch('/api/' + g_project.id, {
                method: 'DELETE'
            }).then(response => {
                if (response.ok) {
                    console.log('Vorschlag gelöscht.');
                    location.reload();
                }
            });
        }
        else {
            console.log("Nein");
            $('#details-dialog-loeschen').blur();
        }
    });
    $('#details-dialog-freischalten').unbind('click');
    $('#details-dialog-freischalten').bind("click", function () {
        fetch('/api/' + g_project.id + '/1', {
            method: 'PUT'
        }).then(response => {
            console.log(response);
            if (response.ok) {
                g_project.status = 1;
                console.log('Vorschlag akkzeptiert.');
                $('#details-dialog-freischalten').hide();
                $('#details-dialog-bearbeiten').show();
                $('#details-dialog-loeschen').hide();
                $("#details-dialog-speichern").hide();
                $("#details-dialog-details").prop("disabled", true);
                $("#fileUpload").hide();
                $("#details-dialog-delete-image").hide();
            }
            else {
                console.log(response.statusText);
            }
        });
    });
    $('#details-dialog-bearbeiten').unbind('click');
    $('#details-dialog-bearbeiten').bind("click", function () {
        fetch('/api/' + g_project.id + '/0', {
            method: 'PUT'
        }).then(response => {
            console.log(response);
            if (response.ok) {
                g_project.status = 0;
                console.log('Vorschlag wieder auf 0.');
                $('#details-dialog-bearbeiten').hide();
                $('#details-dialog-freischalten').show();
                $('#details-dialog-loeschen').show();
                $("#details-dialog-speichern").show();
                $("#details-dialog-details").prop("disabled", false);
                $("#fileUpload").show();
                $("#details-dialog-delete-image").show();
            }
            else {
                console.log(response.statusText);
            }
        });
    });
    $("#likeButton").removeClass("setVoted");
    $("#favoriteButton").removeClass("setVoted");
    if (sessionStorage.Id) {
        var projectRating = userRatings.filter(r => r.projectId == g_project.id);
        $("#leftFavorites").html(leftFavorites.toString());
        console.log("Project Rating:", projectRating);
        console.log("Verbleibende Favorite: ", leftFavorites);
        VoteOn();
        if (projectRating.length > 0) {
            if (projectRating[0].like == true) {
                $("#likeButton").addClass("setVoted");
            }
            if (projectRating[0].favorite == true) {
                $("#favoriteButton").addClass("setVoted");
            }
        }
    }
    else {
        VoteOff();
        $("#leftFavorites").html("0");
    }
    $("#likeButton").off("click");
    $("#likeButton").bind("click", function (e) {
        projectRating = userRatings.filter(r => r.projectId == g_project.id);
        if ($("#likeButton").hasClass("setVoted")) {
            $("#likeButton").removeClass("setVoted");
            $("#likeButton").blur();
            var index = userRatings.indexOf(projectRating[0]);
            projectRating[0].like = false;
            VoteOff();
            PutChangedRating(index, projectRating[0], VoteOn);
        }
        else {
            $("#likeButton").addClass("setVoted");
            if (projectRating.length == 0) {
                VoteOff();
                PostNewRating(g_project, { id: undefined, userId: sessionStorage.Id, projectId: g_project.id, like: true, favorite: null }, VoteOn);
            }
            else {
                var index = userRatings.indexOf(projectRating[0]);
                VoteOff();
                projectRating[0].like = true;
                PutChangedRating(index, projectRating[0], VoteOn);
            }
        }
    });
    $("#favoriteButton").off("click");
    $("#favoriteButton").bind("click", function (e) {
        projectRating = userRatings.filter(r => r.projectId == g_project.id);
        if ($("#favoriteButton").hasClass("setVoted")) {
            $("#favoriteButton").removeClass("setVoted");
            $("#favoriteButton").blur();
            var index = userRatings.indexOf(projectRating[0]);
            projectRating[0].favorite = false;
            leftFavorites++;
            VoteOff();
            PutChangedRating(index, projectRating[0], VoteOn);
            ShowUserInfo(true);
        }
        else if (leftFavorites > 0) {
            leftFavorites--;
            $("#favoriteButton").addClass("setVoted");
            if (projectRating.length == 0) {
                VoteOff();
                PostNewRating(g_project, { id: undefined, userId: sessionStorage.Id, projectId: g_project.id, like: null, favorite: true }, VoteOn);
            }
            else {
                var index = userRatings.indexOf(projectRating[0]);
                projectRating[0].favorite = true;
                VoteOff();
                PutChangedRating(index, projectRating[0], VoteOn);
            }
            ShowUserInfo(true);
        }
        else {
            console.log("Keine Favoriten mehr vorhanden!");
        }
    });
    $("#details-dialog-speichern").unbind("click");
    if (g_project.status == 0) {
        if (sessionStorage.Manager == "yes") {
            $('#details-dialog-freischalten').show();
        }
        else {
            $('#details-dialog-freischalten').hide();
        }
        $('#details-dialog-bearbeiten').hide();
        $('#details-dialog-loeschen').show();
        $("#details-dialog-speichern").unbind();
        $("#details-dialog-speichern").bind("click", function () {
            $("#details-dialog-speichern").text("Wird gespeichert...");
            $("#details-dialog-speichern").prop("disabled", true);
            console.log("Details speichern");
            console.log(getMassnahme(g_project) + " " + g_project.strasse);
            console.log(getMassnahme(primitive.metadata) + " " + primitive.metadata.strasse);
            primitive.metadata = SaveDetails(g_project);
        });
        $("#details-dialog-speichern").show();
        $("#details-dialog-details").prop("disabled", false);
        $("#fileUpload").show();
        $("#details-dialog-delete-image").show();
    }
    else {
        if (sessionStorage.Manager == "yes") {
            $('#details-dialog-bearbeiten').show();
        }
        else {
            $('#details-dialog-bearbeiten').hide();
        }
        $('#details-dialog-freischalten').hide();
        $('#details-dialog-loeschen').hide();
        $("#details-dialog-speichern").hide();
        $("#details-dialog-details").prop("disabled", true);
        $("#fileUpload").hide();
        $("#details-dialog-delete-image").hide();
    }
    ShowProjectImages(g_project.bilder);
}
//https://entsiegelung.blob.core.windows.net/pictures/
function ShowProjectImages(images) {
    var imageHtml = '<div class="carousel-item" ><img class="d-block w-100" src = "/Upload/*.jpeg" ></div>';
    $("#BilderCarousel").empty();
    var i = 0;
    if (images.length > 0) {
        for (var bild of images) {
            if (i == 0) {
                $("#BilderCarousel").append(`<div class="carousel-item active" ><img class="d-block w-100" src = "https://entsiegelung.blob.core.windows.net/pictures/${bild.name}.jpeg" data-id="${bild.id}" ></div>`);
                //$("#BilderCarousel").append('<div class="carousel-item active" ><img class="d-block w-100" src = "/upload/' + bild.name + '.jpeg" data-id="' + bild.id + '" ></div>');
            }
            else {
                $("#BilderCarousel").append(`<div class="carousel-item" ><img class="d-block w-100" src = "https://entsiegelung.blob.core.windows.net/pictures/${bild.name}.jpeg" data-id="${bild.id}" ></div>`);
                //$("#BilderCarousel").append('<div class="carousel-item" ><img class="d-block w-100" src = "/upload/' + bild.name + '.jpeg" data-id="' + bild.id + '"></div>');
            }
            i++;
        }
        $('#details-dialog-delete-image').show();
    }
    else {
        $("#BilderCarousel").append('<div class="carousel-item active" ><img class="d-block w-100" src = "/csm_asphalt_pflanze_68b895e5bb.jpg" ></div>');
        $('#details-dialog-delete-image').hide();
    }
}
function DeleteImageFromCarousel() {
    var id = GetActiveImageIdFromCarousel();
    console.log("Image Id: " + id);
    if (id != null) {
        console.log(g_project.bilder);
        $.ajax('/api/bilder/' + id, {
            type: 'DELETE',
            success: function () {
                console.log('DeleteBild success');
                g_project.bilder.splice(GetImageIndex(id), 1);
                console.log(g_project.bilder);
                ShowProjectImages(g_project.bilder);
            },
            error: function (data) {
                console.log('error: ' + data);
            }
        });
    }
}
function GetImageIndex(imageId) {
    for (var i = 0; i < g_project.bilder.length; i++) {
        if (g_project.bilder[i].id == imageId) {
            return i;
        }
    }
}
function GetActiveImageIdFromCarousel() {
    var carousel = document.getElementById("BilderCarousel");
    //console.log(carousel);
    var active = carousel.getElementsByClassName("active");
    //console.log(active);
    //console.log(active.item(0).children[0]);
    return active.item(0).children[0].getAttribute("data-id");
}
function SaveDetails(project) {
    project.details = $("#details-dialog-details").val();
    project.bsv = $("#bsv").prop("checked");
    project.kub = $("#kub").prop("checked");
    project.bpf = $("#bpf").prop("checked");
    project.pzuB = $("#pzub").prop("checked");
    project.pentsV = $("#pentsv").prop("checked");
    project.vzuG = $("#vzug").prop("checked");
    project.div = $("#div").prop("checked");
    project.vbeet = $("#vbeet").prop("checked");
    project.pp = $("#pp").prop("checked");
    project.ug = $("#ug").prop("checked");
    project.azuX = $("#azux").prop("checked");
    project.gwPI = $("#gwpi").prop("checked");
    project.ruF = $("#ruf").prop("checked");
    PutChangedProject(project);
    console.log("SaveDetails");
    console.log(project);
    return project;
}
var tooltipTemplate = `
<div id="hoverTooltip" class="badge badge-info" style="white-space:nowrap">
    <span style="font-size:1rem">{title}</span>
</div>
`;
function iPrimitiveHovered(e) {
    //https://docs.microsoft.com/en-us/bingmaps/v8-web-control/map-control-concepts/infoboxes/expanding-tooltip-infobox
    tooltip.setOptions({
        location: e.location,
        htmlContent: tooltipTemplate.replace("{title}", e.target.metadata.strasse),
        visible: true
    });
    $("#hoverTooltip").off("click");
    $("#hoverTooltip").bind("click", function (f) {
        e.primitive.click._handlers[0](e);
    });
}
function closeTooltip() {
    //Close the tooltip.
    tooltip.setOptions({
        visible: false
    });
}
function getMassnahme(p) {
    if (p.bpf || p.pzuB) {
        return "Bäume pflanzen";
    }
    else if (p.kub || p.pentsV || p.azuX || p.gwPI) {
        return "Bauliche Entsiegelung";
    }
    else if (p.pp || p.ug || p.vbeet) {
        return "Versickerungs<wbr>beet";
    }
    else if (p.vzuG) {
        return "Grünflächen schaffen";
    }
    else if (p.bsv) {
        return "Baumscheiben-Vergrößerung";
    }
    return "sonstiges";
}
function getMassnahmenId(p) {
    if (p.bpf || p.pzuB) {
        return 0;
    }
    else if (p.kub || p.pentsV || p.azuX || p.gwPI) {
        return 3;
    }
    else if (p.pp || p.ug || p.vbeet) {
        return 2;
    }
    else if (p.vzuG) {
        return 4;
    }
    else if (p.bsv) {
        return 1;
    }
    return -1;
}
function getSvgId(p) {
    if (p.bpf || p.pzuB) {
        return "#svgBaum";
    }
    else if (p.kub || p.pentsV || p.azuX || p.gwPI) {
        return "#svgBau";
    }
    else if (p.pp || p.ug || p.vbeet) {
        return "#svgVbeet";
    }
    else if (p.pp || p.ug || p.vbeet) {
        return "#svgVbeet";
    }
    else if (p.vzuG) {
        return "#svgGruen";
    }
    else if (p.bsv) {
        return "#svgBSV";
    }
    return "";
}
function postError(c, s) {
    var urlstring = "/Error/PostError?user=" + (sessionStorage.Id ? sessionStorage.Id : "") + "&code=" + c;
    var options = {
        url: urlstring,
        type: "POST",
        data: JSON.stringify(s),
        contentType: "application/json; charset=utf-8",
        beforeSend: function (request) {
            console.log("Sending Error...");
            console.log(urlstring);
        },
        success: function (response) {
            console.log("Error was sent.");
        },
        error: function (xhr) {
            console.log("Error couldn't be sent");
        }
    };
    $.ajax(options);
}
//# sourceMappingURL=bingmaps.js.map