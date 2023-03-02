<script lang="ts">
	import { walter_get } from '$WalterServices/requests';
	import { WalterKontakte, WalterHeaderList } from '$WalterComponents';
	import type { WalterPersonEntry } from '$WalterTypes';

	const url = 'kontakte';

	const rows: Promise<WalterPersonEntry[]> = walter_get(`/api/${url}`);
</script>

<WalterHeaderList {url} title="Kontakte" />
<WalterKontakte search {rows} />
