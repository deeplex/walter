<script lang="ts">
	import { Vertraege, WalterHeaderList } from '$components';
	import type { VertragEntry } from '$types';
	import { walter_get } from '$services/requests';

	const url = 'vertraege';

	const rows: Promise<VertragEntry[]> = walter_get(`/api/${url}`);
</script>

<WalterHeaderList {url} title="Verträge" />
<Vertraege search {rows} />
