<script lang="ts">
	import { walter_get } from '$services/utils';
	import { Kontakte, WalterHeader } from '$components';
	import type { PersonEntry } from '$types';

	const rows: Promise<PersonEntry[]> = walter_get('/api/kontakte');
</script>

<WalterHeader title="Kontakte" />
<Kontakte search {rows} />
