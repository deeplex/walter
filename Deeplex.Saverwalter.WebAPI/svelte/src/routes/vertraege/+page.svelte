<script lang="ts">
	import { Vertraege, WalterHeader } from '$components';
	import type { VertragEntry } from '$types';
	import { walter_get } from '$services/utils';

	const rows: Promise<VertragEntry[]> = walter_get('/api/vertraege');
</script>

<WalterHeader title="Verträge" />
<Vertraege search {rows} />
