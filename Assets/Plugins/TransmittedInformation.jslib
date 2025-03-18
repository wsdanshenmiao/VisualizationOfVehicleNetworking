mergeInto(LibraryManager.library, {
	SendMessageToParent : function (status, name) {
		window.parent.postMessage({ type:'changeStatus', status : status, name : name }, '*');
	},
});