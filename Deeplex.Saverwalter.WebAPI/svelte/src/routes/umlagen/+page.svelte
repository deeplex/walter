<script lang="ts">
	import { WalterUmlagen, WalterHeaderList } from '$WalterComponents';
	import { walter_get } from '$WalterServices/requests';
	import type { WalterUmlageEntry } from '$WalterTypes';

	const url = 'umlagen';

	const rows: Promise<WalterUmlageEntry[]> = walter_get(`/api/${url}`);
</script>

<WalterHeaderList {url} title="Umlagen" />
<WalterUmlagen search {rows} />
