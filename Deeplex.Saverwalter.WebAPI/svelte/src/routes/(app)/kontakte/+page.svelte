<script lang="ts">
	import { WalterKontakte, WalterHeaderList } from '$WalterComponents';
	import type { PageData } from './$types';

	export let data: PageData;
</script>

<WalterHeaderList title="Kontakte" />
<WalterKontakte search rows={data.rows} />
