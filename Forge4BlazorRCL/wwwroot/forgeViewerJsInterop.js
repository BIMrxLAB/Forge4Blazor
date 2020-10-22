var viewer = {};
var viewerDoc = {};

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

    addMouseMoveEvent: function (dotNetObject, loc) {
        viewer[loc].canvas.addEventListener('mousemove', function (e) {
            //console.log("JS: Generated ", e);
            dotNetObject.invokeMethodAsync('PostMouseLocation', e.layerX, e.layerY);
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

    }
};