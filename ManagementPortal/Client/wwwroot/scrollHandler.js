window.handleScroll = (dotNetHelper, IdScroll) => {
	const element = document.getElementById(IdScroll);
	if (!element) {
		console.log(IdScroll);
		console.error('Element not found');
		return;
	}

	const scrollTop = element.scrollTop;
	const scrollHeight = element.scrollHeight;
	const clientHeight = element.clientHeight;

	const tolerance = 5;

	if (scrollTop + clientHeight >= scrollHeight - tolerance) {
		console.log('Reached bottom, load more data');

		dotNetHelper.invokeMethodAsync('LoadMoreData')
			.then(result => {
				console.log('C# method result:', result);
			})
			.catch(error => {
				console.error('Error invoking C# method:', error);
			});
	}
};
