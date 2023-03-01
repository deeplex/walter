<script lang="ts">
	import { Umlagen, WalterHeader } from '$components';
	import { walter_get } from '$services/utils';
	import type { UmlageEntry } from '$types';

	const rows: Promise<UmlageEntry[]> = walter_get('/api/umlagen');
</script>

<WalterHeader title="Umlagen" />
<Umlagen search {rows} />
