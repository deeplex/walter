<script lang="ts">
	import { Erhaltungsaufwendungen, WalterHeaderList } from '$components';
	import { walter_get } from '$services/requests';
	import type { ErhaltungsaufwendungEntry } from '$types';

	const url = 'erhaltungsaufwendungen';

	const rows: Promise<ErhaltungsaufwendungEntry[]> = walter_get(`/api/${url}`);
</script>

<WalterHeaderList {url} title="Erhaltungsaufwendungen" />
<Erhaltungsaufwendungen search {rows} />
