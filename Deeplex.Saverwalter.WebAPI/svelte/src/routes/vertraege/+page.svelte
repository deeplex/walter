<script lang="ts">
	import { WalterVertraege, WalterHeaderList } from '$WalterComponents';
	import type { WalterVertragEntry } from '$WalterTypes';
	import { walter_get } from '$WalterServices/requests';

	const url = 'vertraege';

	const rows: Promise<WalterVertragEntry[]> = walter_get(`/api/${url}`);
</script>

<WalterHeaderList {url} title="Verträge" />
<WalterVertraege search {rows} />
