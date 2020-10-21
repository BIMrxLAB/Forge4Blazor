var viewer;
var viewerDoc;
window.forgeViewerJsFunctions = {

    startViewer: function (token, loc) {
        var options = {
            env: 'AutodeskProduction',
            accessToken: token,
            location: loc
        };
        Autodesk.Viewing.Initializer(options, function onInitialized() {
            viewer = new Autodesk.Viewing.GuiViewer3D(document.getElementById(options.location));
            viewer.start();
        });
    },

    addMouseMoveEvent: function (dotNetObject) {
        viewer.canvas.addEventListener('mousemove', function (e) {
            //console.log("JS: Generated ", e);
            dotNetObject.invokeMethodAsync('PostMouseLocation', e.layerX, e.layerY);
        });
    },

    loadExtension: async function (ext) {
        await viewer.loadExtension(ext);
    },

    loadFile: async function (file) {
        await viewer.loadModel(file, viewer);
    },

    /////////////////////////////////////////////////////////
    // Load a document from URN
    // https://forge.autodesk.com/blog/switching-viewables-forge-viewer
    /////////////////////////////////////////////////////////
    loadDocument: function (urn) {

        return new Promise((resolve, reject) => {

            const paramUrn = !urn.startsWith('urn:')
                ? 'urn:' + urn
                : urn

            Autodesk.Viewing.Document.load(paramUrn, (doc) => {
                viewerDoc = doc;
                resolve('loadDocument(urn) success.')

            }, (error) => {

                reject(error)
            })
        })
    },

    onDocumentLoadSuccess: function (doc) {
        console.warn('onDocumentLoadSuccess...')
        viewerDoc = doc;
    },

    onDocumentLoadFailure: function (viewerErrorCode) {
        console.warn('onDocumentLoadFailure...')
        console.error('onDocumentLoadFailure() - errorCode:' + viewerErrorCode);
    },

    loadDocumentNode: async function (viewableId) {
        console.warn('loadDocumentNode...')
        var viewables;
        if (viewableId) {
            viewables = viewerDoc.getRoot().findByGuid(viewableId);
        } else {
            viewables = viewerDoc.getRoot().getDefaultGeometry();
        }

        await viewer.loadDocumentNode(viewerDoc, viewables);

    }
};