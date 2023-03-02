<script lang="ts">
	import {
		WalterErhaltungsaufwendungen,
		WalterHeaderList
	} from '$WalterComponents';
	import { walter_get } from '$WalterServices/requests';
	import type { WalterErhaltungsaufwendungEntry } from '$WalterTypes';

	const url = 'erhaltungsaufwendungen';

	const rows: Promise<WalterErhaltungsaufwendungEntry[]> = walter_get(
		`/api/${url}`
	);
</script>

<WalterHeaderList {url} title="Erhaltungsaufwendungen" />
<WalterErhaltungsaufwendungen search {rows} />
