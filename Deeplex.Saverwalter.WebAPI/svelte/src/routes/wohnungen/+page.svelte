<script lang="ts">
	import { WalterHeaderList, Wohnungen } from '$components';
	import { walter_get } from '$services/requests';
	import type { WohnungEntry } from '$types';

	const url = 'wohnungen';

	const rows: Promise<WohnungEntry[]> = walter_get(`/api/${url}`);
</script>

<WalterHeaderList {url} title="Wohnungen" />
<Wohnungen search {rows} />
