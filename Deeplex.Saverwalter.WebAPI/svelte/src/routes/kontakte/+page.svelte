<script lang="ts">
	import { walter_get } from '$services/requests';
	import { Kontakte, WalterHeaderList } from '$components';
	import type { PersonEntry } from '$types';

	const url = 'kontakte';

	const rows: Promise<PersonEntry[]> = walter_get(`/api/${url}`);
</script>

<WalterHeaderList {url} title="Kontakte" />
<Kontakte search {rows} />
