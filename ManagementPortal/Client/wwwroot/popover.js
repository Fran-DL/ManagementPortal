let clickAwayListener = null;

function addClickAwayListener(dotNetHelper) {
	clickAwayListener = (event) => {
		const popover = document.querySelector(".noti-popover");
		if (
			popover &&
			!popover.contains(event.target)
		) {
			dotNetHelper.invokeMethodAsync("ClosePopover");
		}
	};
	document.addEventListener("click", clickAwayListener);
}

function removeClickAwayListener() {
	if (clickAwayListener) {
		document.removeEventListener("click", clickAwayListener);
		clickAwayListener = null;
	}
}
