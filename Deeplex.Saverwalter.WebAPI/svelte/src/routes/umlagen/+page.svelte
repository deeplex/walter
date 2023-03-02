<script lang="ts">
	import { Umlagen, WalterHeaderList } from '$components';
	import { walter_get } from '$services/requests';
	import type { UmlageEntry } from '$types';

	const url = 'umlagen';

	const rows: Promise<UmlageEntry[]> = walter_get(`/api/${url}`);
</script>

<WalterHeaderList {url} title="Umlagen" />
<Umlagen search {rows} />
