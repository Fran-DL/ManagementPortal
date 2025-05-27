function getClientTimezone() {
	return Intl.DateTimeFormat().resolvedOptions().timeZone;
}