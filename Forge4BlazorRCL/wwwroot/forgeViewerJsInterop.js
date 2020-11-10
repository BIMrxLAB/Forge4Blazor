var viewer = {};
var viewerDoc = {};
var snapper = {};

window.forgeViewerJsFunctions = {

    startViewer: function (token, loc) {
        var options = {
            env: 'AutodeskProduction',
            accessToken: token,
            location: loc
        };
        Autodesk.Viewing.Initializer(options, function onInitialized() {
            viewer[loc] = new Autodesk.Viewing.GuiViewer3D(document.getElementById(options.location));
            viewer[loc].start();
        });
    },

    addMouseClickEvent: function (dotNetObject, loc) {
        viewer[loc].canvas.addEventListener('click', function (e) {
            console.warn(e);
            //https://stackoverflow.com/questions/48142782/how-to-get-x-y-coordinates-in-2d-drawing-of-forge-viewer
            let stuff = viewer[loc].clientToWorld(e.layerX, e.layerY, true);
            console.warn(stuff);

            if (!(snapper[loc] == null || typeof snapper[loc] === 'undefined')) {
                if (snapper[loc].isSnapped()) {
                    const result = snapper[loc].getSnapResult();
                    console.warn(result);
                    dotNetObject.invokeMethodAsync('PostMouseClickLocation', e.layerX, e.layerY, stuff.point.x, stuff.point.y, result.geomVertex.x, result.geomVertex.y, result.geomVertex.z);

                    const { SnapType } = Autodesk.Viewing.MeasureCommon;
                    switch (result.geomType) {
                        case SnapType.SNAP_VERTEX:
                        case SnapType.SNAP_MIDPOINT:
                        case SnapType.SNAP_INTERSECTION:
                        case SnapType.SNAP_CIRCLE_CENTER:
                        case SnapType.RASTER_PIXEL:
                        default:
                            // Do not snap to other types
                            break;
                    }
                }
            } else {
                dotNetObject.invokeMethodAsync('PostMouseClickLocation', e.layerX, e.layerY, stuff.point.x, stuff.point.y, 0, 0, 0);
            }
        });
    },

    addMouseMoveEvent: function (dotNetObject, loc) {
        viewer[loc].canvas.addEventListener('mousemove', function (e) {
            console.log("JS: Generated mousemove", e);

            let stuff = viewer[loc].clientToWorld(e.layerX, e.layerY, true);
            console.warn(stuff);

            //https://d1r98t40ydopvd.cloudfront.net/blog/snappy-viewer-tools
            if (!(snapper[loc] == null || typeof snapper[loc] === 'undefined')) {
                snapper[loc].indicator.clearOverlays();
                if (snapper[loc].isSnapped()) {
                    const result = snapper[loc].getSnapResult();

                    dotNetObject.invokeMethodAsync('PostMouseMoveLocation', e.layerX, e.layerY, stuff.point.x, stuff.point.y, result.geomVertex.x, result.geomVertex.y, result.geomVertex.z);

                    const { SnapType } = Autodesk.Viewing.MeasureCommon;
                    switch (result.geomType) {
                        case SnapType.SNAP_VERTEX:
                        case SnapType.SNAP_MIDPOINT:
                        case SnapType.SNAP_INTERSECTION:
                        case SnapType.SNAP_CIRCLE_CENTER:
                        case SnapType.RASTER_PIXEL:
                            // console.log('Snapped to vertex', result.geomVertex);
                            snapper[loc].indicator.render(); // Show indicator when snapped to a vertex
                            //this._update(result.geomVertex);
                            break;
                        case SnapType.SNAP_EDGE:
                        case SnapType.SNAP_CIRCULARARC:
                        case SnapType.SNAP_CURVEDEDGE:
                            // console.log('Snapped to edge', result.geomEdge);
                            break;
                        case SnapType.SNAP_FACE:
                        case SnapType.SNAP_CURVEDFACE:
                            // console.log('Snapped to face', result.geomFace);
                            break;
                    }
                }
            } else {
                dotNetObject.invokeMethodAsync('PostMouseMoveLocation', e.layerX, e.layerY, stuff.point.x, stuff.point.y, 0, 0, 0);
            }
        });
    },

    loadExtension: async function (ext, loc) {
        await viewer[loc].loadExtension(ext);
    },

    loadFile: async function (file, loc) {
        viewerDoc[loc] = {};
        await viewer[loc].loadModel(file, viewer[loc]);
    },

    /////////////////////////////////////////////////////////
    // Load a document from URN
    // https://forge.autodesk.com/blog/switching-viewables-forge-viewer
    /////////////////////////////////////////////////////////
    loadDocument: function (urn, loc) {

        return new Promise((resolve, reject) => {

            const paramUrn = !urn.startsWith('urn:')
                ? 'urn:' + urn
                : urn

            Autodesk.Viewing.Document.load(paramUrn, (doc) => {
                viewerDoc[loc] = doc;
                resolve('loadDocument(urn) success.')

            }, (error) => {

                reject(error)
            })
        })
    },

    loadDocumentNode: async function (viewableId, loc) {
        console.warn('loadDocumentNode...')
        var viewables;
        if (viewableId) {
            viewables = viewerDoc[loc].getRoot().findByGuid(viewableId);
        } else {
            viewables = viewerDoc[loc].getRoot().getDefaultGeometry();
        }

        await viewer[loc].loadDocumentNode(viewerDoc[loc], viewables);

    },

    makeSnapper: function (loc) {
        if ((viewer[loc] == null || typeof viewer[loc] === 'undefined')) {
            console.warn('make snapper - no viewer.' + loc);
        } else {
            snapper[loc] = new Autodesk.Viewing.Extensions.Snapping.Snapper(viewer[loc], { renderSnappedGeometry: true, renderSnappedTopology: true });
        }
    },

    destroySnapper: function (loc) {
        if ((snapper[loc] == null || typeof snapper[loc] === 'undefined')) {
            console.warn('destroy snapper - no snapper.' + loc);
        } else {
            snapper[loc] = null;
        }
    },

    registerAndActivateSnapper: function (loc) {
        if ((viewer[loc] == null || typeof viewer[loc] === 'undefined')) {
            console.warn('register snapper - no viewer.' + loc);
        } else {
            console.warn('register snapper');
            viewer[loc].toolController.registerTool(snapper[loc]);
            viewer[loc].toolController.activateTool(snapper[loc].getName());
        }
    },

    deregisterAndDeactivateSnapper: function (loc) {
        if ((viewer[loc] == null || typeof viewer[loc] === 'undefined')) {
            console.warn('deregister snapper - no viewer.' + loc);
        } else {
            console.warn('deregister snapper');
            viewer[loc].toolController.deactivateTool(snapper[loc].getName());
            viewer[loc].toolController.deregisterTool(snapper[loc]);
        }
    }
};